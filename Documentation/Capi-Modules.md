# Modules used in project
#### _Table of contents_
## CAPI Modules
+ [CAPI.Agent](#CAPIAgent)
+ [CAPI.Agent.Abstractions](#CAPIAgentAbstractions)
+ [CAPI.Common](#CAPICommon)
+ [CAPI.Common.Abstractions](#CAPICommonAbstractions)
+ [CAPI.Console.Net](#CAPIConsoleNet)
+ [CAPI.Dicom](#CAPIDicom)
+ [CAPI.Dicom.Abstractions](#CAPIDicomAbstractions)
+ [CAPI.Extensions](#CAPIExtensions)
+ [CAPI.General](#CAPIGeneral)
+ [CAPI.General.Abstractions](#CAPIGeneralAbstractions)
+ [CAPI.ImageProcessing](#CAPIImageProcessing)
+ [CAPI.ImageProcessing.Abstractions](#CAPIImageProcessingAbstractions)
+ [CAPI.LookUpTableGenerator](#CAPILookUpTableGenerator)
+ [CAPI.Tests](#CAPITests)
+ [CAPI.UAT](#CAPIUAT)

## CAPI Dependencies
+ [ClearCanvas.Common](#ClearCanvasCommon)
+ [ClearCanvas.Dicom](#ClearCanvasDicom)
---
## CAPI.Agent



## CAPI.Agent.Abstractions
## CAPI.Common
## CAPI.Common.Abstractions
## CAPI.Console.Net

Entry point for the project. It is recommended for this to be used only for testing. In production settings `CAPI.Service` is to be used. 

if run with parameter "uat" i.e. `capi.console.net.exe uat` it will call `CAPI.UAT.TestRunner`

if no parameters are passed to `capi.console.net.exe` file, `CAPI.Agent` is called.

## CAPI.Dicom
## CAPI.Dicom.Abstractions
## CAPI.Extensions
## CAPI.General
## CAPI.General.Abstractions
## CAPI.ImageProcessing
## CAPI.ImageProcessing.Abstractions
## CAPI.LookUpTableGenerator
## CAPI.Tests
## CAPI.UAT

# CAPI Dependencies
## ClearCanvas.Common
## ClearCanvas.Dicom



