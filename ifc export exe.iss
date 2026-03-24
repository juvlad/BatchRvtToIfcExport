[Setup]
AppId={{1AD3D195-F365-4678-A4A0-DC8381E7839B}}}
AppName=BatchRvtToifcExport
AppVersion=1.0
DefaultDirName={commonappdata}\Autodesk\Revit\Addins\2024\BatchRvtToifcExport
DefaultGroupName=BatchRvtToifcExport
; Запрос прав администратора для записи в ProgramData
PrivilegesRequired=admin
Compression=lzma
SolidCompression=yes
OutputDir=userdocs:\InnoSetupOutput

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Files]
; 1. Копируем исполняемые файлы и логику в ProgramData
; {app} здесь ссылается на DefaultDirName из блока [Setup]
Source: "C:\Users\VlaD\Desktop\BIM\C#\ifc export\bin\Release\net48\BatchRvtToIfcExport2024.dll"; DestDir: "{app}"; Flags: ignoreversion


; 2. Копируем манифест .addin в AppData текущего пользователя
; {userappdata} разворачивается в C:\Users\Имя\AppData\Roaming
Source: "C:\Users\VlaD\Desktop\BIM\C#\ifc export\BatchRvtToifcExport.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2024"; Flags: ignoreversion

[Code]
// Если нужно динамически менять путь в .addin файле при установке, 
// это можно сделать через секцию [Strings] или скриптом, 
// но проще заранее прописать в .addin относительный или фиксированный путь к DLL.