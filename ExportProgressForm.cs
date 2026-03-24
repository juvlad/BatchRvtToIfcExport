using System;
using System.Drawing;
using System.Windows.Forms;

namespace IfcExport2024
{
    public class ExportProgressForm : Form
    {
        private readonly ProgressBar _progressBar;
        private readonly Label _label;
        private readonly int _total;
        private int _current;

        public ExportProgressForm(int totalFiles)
        {
            _total = Math.Max(1, totalFiles);

            Text = "Экспорт моделей в IFC";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ControlBox = false;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(500, 120);

            _label = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Подготовка экспорта IFC..."
            };

            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = _total,
                Step = 1,
                Style = ProgressBarStyle.Continuous
            };

            Controls.Add(_progressBar);
            Controls.Add(_label);

            FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                }
            };
        }

        public void Step(string currentFileName)
        {
            _current++;
            if (_current > _total) _current = _total;

            _label.Text = $"Экспортируется в IFC: {currentFileName} ({_current}/{_total})";
            _progressBar.Value = _current;

            Application.DoEvents();
        }
    }
}

