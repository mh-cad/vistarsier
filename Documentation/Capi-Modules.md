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

This module gets called by project entry point e.g. `CAPI.Console.Net` or `CAPI.Service`, controls the whole process of monitoring HL7 and manual folders where **recipes** or **files with accession numbers as their filenames** are dumped. Agent will collect files and check against the database and will either add the accession number to pending cases if new or will update the accession in database to pending if already existing.

## CAPI.Agent.Abstractions

Interfaces for `CAPI.Agent` module

## CAPI.Common
## CAPI.Common.Abstractions

Interfaces for `CAPI.Common` module

## CAPI.Console.Net

This module is one of the entry points of the project. It is recommended that it is used only for testing. In production `CAPI.Service` is to be used. 

if run with parameter "uat" i.e. `capi.console.net.exe uat` it will call `CAPI.UAT.TestRunner`

if no parameters are passed to `capi.console.net.exe` file, `CAPI.Agent` is called.

## CAPI.Dicom
## CAPI.Dicom.Abstractions

Interfaces for `CAPI.Dicom` module

## CAPI.Extensions

This module contains extensions methods to enhance functionality of .NET framework libraries used across the project.

## CAPI.General

This module contains helper methods for `FileSystem` functions such as _Checking if directory exists and contains files_ or _Check if an array of files all exist_ etc. as well as `Process` functions such as _building  processes to call exe files_ etc.

## CAPI.General.Abstractions

Interfaces for `CAPI.General` module

## CAPI.ImageProcessing
## CAPI.ImageProcessing.Abstractions

Interfaces for `CAPI.ImageProcessing` module

## CAPI.LookUpTableGenerator
## CAPI.Tests
## CAPI.UAT

# CAPI Dependencies
## ClearCanvas.Common
## ClearCanvas.Dicom



