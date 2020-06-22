;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"
  

;--------------------------------
;General

  ;Name and file
  Name "VisTarsier 2.0"
  OutFile "VTSetup_2.exe"

  ;Default installation folder
  InstallDir "$EXEDIR\VisTarsier"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\VisTarsier" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel admin

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "..\LICENSE.MD"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "VisTarsierWeb" SecWeb

  SetOutPath "$INSTDIR\web"
  
  ;ADD YOUR OWN FILES HERE...
  File /r "deployment\web\*"

SectionEnd

Section "VisTarsierService" SecService
  SectionIn RO
  SetOutPath "$INSTDIR\service\"
  File /r "deployment\service\*"
  SetOutPath "$INSTDIR\installers\" 
  File /r "deployment\installers\*"
  
  ;Store installation folder
  WriteRegStr HKCU "Software\VisTarsier" "" $INSTDIR
  
  ; Install a service - ServiceType own process - StartType automatic - NoDependencies - Logon as System Account
  SimpleSC::InstallService "VisTarsier" "VisTarsier" "16" "2" "$INSTDIR\service\VisTarsier.Service.exe" "" "" "NT AUTHORITY\LOCAL SERVICE"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
  ExecWait '$INSTDIR\installers\dotnet.exe'
  ExecWait '$INSTDIR\installers\vcredist_x64.exe'
  ExecWait '"$INSTDIR\service\VisTarsier.ConfigApp.exe"'
  
  ;---------------------------------------------------------------------------
  ; Web services (these are in this section as they can fail silently 
  ; if there was no web install and we want them to happen after the settings)  
  
  ; Install web UI service 
  SetOutPath "$INSTDIR\web\nodejs\" 
  ExecWait '$INSTDIR\web\nodejs\nodevars.cmd'
  ExecWait '$INSTDIR\web\nodejs\qckwinsvc.cmd --name VisTarsier-Web-UI --description VisTarsierWebInterface --script $INSTDIR\web\nodejs\server.js --startImmediately'
  ; Install web backend service
  ExecWait '$INSTDIR\web\nssm.exe install VisTarsier.Web.Dicom "$INSTDIR\web\restfuldicom\python-3.7.4-embed-amd64\python.exe"'
  ExecWait '$INSTDIR\web\nssm.exe set VisTarsier.Web.Dicom AppDirectory "$INSTDIR\web\restfuldicom"'
  ExecWait '$INSTDIR\web\nssm.exe set VisTarsier.Web.Dicom AppParameters rest-dicom.py'
  ExecWait '$INSTDIR\web\nssm.exe start VisTarsier.Web.Dicom'
  ;---------------------------------------------------------------------------
  
  SimpleSC::StartService "VisTarsier" "" 30
  Pop $0
  SimpleSC::StartService "VisTarsier.Web.UI" "" 30
  Pop $0
  SimpleSC::StartService "VisTarsier.Web.Dicom" "" 30
  Pop $0
  Pop $0

  
SectionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_SecService ${LANG_ENGLISH} "The main VisTarsier Windows service"
  LangString DESC_SecWeb ${LANG_ENGLISH} "VisTarsier Webserver"

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecService} $(DESC_SecService)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecWeb} $(DESC_SecWeb)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ; The uninstaller has been moved to temp so we can delete.
  Delete "$INSTDIR\Uninstall.exe"
  
  ; Stop 
  SimpleSC::StopService "VisTarsier" "" 30
  Pop $0
  SimpleSC::RemoveService "VisTarsier"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  SimpleSC::StopService "VisTarsier-Web-UI" "" 30
  Pop $0
  SimpleSC::RemoveService "VisTarsier-Web-UI"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  SimpleSC::StopService "VisTarsier-Postgres" "" 30
  Pop $0
  SimpleSC::RemoveService "VisTarsier-Postgres"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  
  
  ;Remove dicom service
  ExecWait '$INSTDIR\web\nssm.exe stop VisTarsier.Web.Dicom confirm'
  ExecWait '$INSTDIR\web\nssm.exe remove VisTarsier.Web.Dicom confirm'
  ExecWait '$INSTDIR\web\nodejs\qckwinsvc.cmd --uninstall --name VisTarsier-Web-UI --script $INSTDIR\web\nodejs\server.js'

  RMDir /r /REBOOTOK "$INSTDIR\cases"
  RMDir /r /REBOOTOK "$INSTDIR\cfg"
  RMDir /r /REBOOTOK "$INSTDIR\img"
  RMDir /r /REBOOTOK "$INSTDIR\log"
  RMDir /r /REBOOTOK "$INSTDIR\service"
  RMDir /r /REBOOTOK "$INSTDIR\web"
  RMDir /r /REBOOTOK "$INSTDIR\installers"
 
  RMDir $INSTDIR

  DeleteRegKey /ifempty HKCU "Software\VisTarsier"

SectionEnd