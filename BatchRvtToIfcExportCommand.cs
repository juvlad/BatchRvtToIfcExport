using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

[Transaction(TransactionMode.Manual)]
public class ExportToIFC : IExternalCommand
{
    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        UIApplication uiapp = commandData.Application;
        var app = uiapp.Application;

        // Ask user for source folder
        string sourceFolder = GetFolder("Выберите папку с RVT моделями");
        if (string.IsNullOrEmpty(sourceFolder))
            return Result.Cancelled;

        // Ask user for export folder
        string exportFolder = GetFolder("Выберите папку для экспорта IFC");
        if (string.IsNullOrEmpty(exportFolder))
            return Result.Cancelled;

        string[] rvtFiles = Directory.GetFiles(sourceFolder, "*.rvt");

        if (!rvtFiles.Any())
        {
            TaskDialog.Show("Ошибка", "В выбранной папке нет файлов RVT.");
            return Result.Failed;
        }

        // Окно прогресса (полоса, без крестика).
        // Привязываем его к главному окну Revit, чтобы оно сворачивалось/разворачивалось вместе с ним.
        var progressForm = new IfcExport2024.ExportProgressForm(rvtFiles.Length);
        var revitOwner = new RevitWindowWrapper(uiapp.MainWindowHandle);
        progressForm.Show(revitOwner);
        Application.DoEvents();

        try
        {
            foreach (string file in rvtFiles)
            {
                try
                {
                    // Open model
                    ModelPath mp = ModelPathUtils.ConvertUserVisiblePathToModelPath(file);
                    OpenOptions options = new OpenOptions();
                    Document doc = app.OpenDocumentFile(mp, options);

                    // обновляем прогресс‑бар
                    string fileName = Path.GetFileName(file);
                    progressForm.Step(fileName);

                    // Prepare IFC export options
                    IFCExportOptions ifcOptions = new IFCExportOptions();
                    // Здесь можно настроить дополнительные параметры экспорта IFC при необходимости.

                    // Имя файла без расширения; Revit сам добавит .ifc
                    string exportFileName = Path.GetFileNameWithoutExtension(file);

                    // IFC‑экспорт в API модифицирует документ (записывает GUID и т.п.),
                    // поэтому требуется открытая транзакция.
                    using (Transaction t = new Transaction(doc, "Batch IFC Export"))
                    {
                        t.Start();
                        bool ok = doc.Export(exportFolder, exportFileName, ifcOptions);
                        t.Commit();

                        if (!ok)
                        {
                            throw new InvalidOperationException("Revit вернул false при экспорте IFC.");
                        }
                    }

                    doc.Close(false);
                }
                catch (Exception ex)
                {
                    // Показываем ошибку, чтобы понять, почему файл не экспортируется
                    TaskDialog.Show("Ошибка экспорта IFC", $"Файл: {file}{Environment.NewLine}{ex.Message}");
                }
            }
        }
        finally
        {
            // закрываем прогресс‑бар даже при ошибке
            if (progressForm != null && !progressForm.IsDisposed)
            {
                progressForm.Close();
                progressForm.Dispose();
            }
        }

        TaskDialog.Show("Готово", "Все модели успешно экспортированы в IFC.");
        return Result.Succeeded;
    }

    /// <summary>
    /// Simple Windows folder picker
    /// </summary>
    private string GetFolder(string title)
    {
        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        {
            dialog.Description = title;
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
        }

        return null;
    }
}

/// <summary>
/// Обёртка для основного окна Revit, чтобы сделать его владельцем формы прогресса.
/// Это позволяет сворачивать/разворачивать прогресс‑бар вместе с Revit.
/// </summary>
public class RevitWindowWrapper : IWin32Window
{
    public IntPtr Handle { get; }

    public RevitWindowWrapper(IntPtr handle)
    {
        Handle = handle;
    }
}

