# Modules used in project
#### _Table of contents_
## CAPI Modules
+ [CAPI.Cmd](#CAPICmd)
+ [CAPI.Common](#CAPICommon)
+ [CAPI.Dicom](#CAPIDicom)
+ [CAPI.Extensions](#CAPIExtensions)
+ [CAPI.MS](#CAPIMS)
+ [CAPI.NiftiLib](#CAPINiftiLib)
+ [CAPI.Service](#CAPIService)
+ [CAPI.Tests](#CAPITests)

## CAPI Dependencies

+ [ClearCanvas.Common](#ClearCanvasCommon)
+ [ClearCanvas.Dicom](#ClearCanvasDicom)
+ [log4net](#log4net)
+ [MathNet.Numerics](#MathNetNumerics)
+ [Newtonsoft.Json](#NewtonsoftJson)
---
## Third Party Tools

- [ANTs](#ANTs)
- [Brain Suite](#Brain-Suite)
- [CMTK](#CMTK)
- [dcm2niix](#dcm2niix)
- [img2dcm](#img2dcm)



## CAPI.Cmd

This module is the command-line version of CAPI. The command line version is primarily aimed at using the NIfTi format and is the simplest example of the CAPI software.

## CAPI.Common

The `CAPI.Common` module contains basic helpers and data structures which are commonly used throughout the code base.

## CAPI.Dicom

Interfaces for `CAPI.Dicom` module

## CAPI.Extensions

This module contains extensions methods to enhance functionality of .NET framework libraries used across the project.

## CAPI.General

This module contains helper methods for `FileSystem` functions such as _Checking if directory exists and contains files_ or _Check if an array of files all exist_ etc. as well as `Process` functions such as _building  processes to call exe files_ etc.

## CAPI.NiftiLib
Library which includes NIfTI file interfaces and functions for processing NIfTI objects.

## CAPI.Service
Main Windows service for running CAPI as part of a PACs system. 

## CAPI.Tests
Unit test suite. Can always do with extending...

# CAPI Dependencies
## ClearCanvas.Common
## ClearCanvas.Dicom
ClearCanvas is a C# DICOM viewer. We're using some DICOM features.
Souce code: https://clearcanvas.github.io/

# Third Party Tools

## ANTs
Advanced normalisation tools.
Source code: http://stnava.github.io/ANTs/

## Brain Suite

Download: http://forums.brainsuite.org/download/

Brain Surface Extractor

- bse.exe
- version 18a
- Compiled at 22:42:52 on Feb 28 2018 (build #:3115)

Bias Field Corrector

- bfc.exe
- version 18a
- Compiled at 22:42:52 on Feb 28 2018 (build #:3115)

Source code: http://forums.brainsuite.org/download/

## CMTK

NITRC Computational Morphometry Toolkit (CMTK)
- Support: https://www.nitrc.org/projects/cmtk
- Download: https://www.nitrc.org/frs/download.php/8212/CMTK-3.3.1-Windows-AMD64.zip
- Version 3.3.1 (24-1-2016)
- Registration
  - registration.exe
- Reslicing
  - reformatx.exe
  
  Source code: https://www.nitrc.org/frs/?group_id=212

## dcm2niix

CMTK dcm2niix tool - part of MRIcroGL package

- dcm2niix.exe
- https://www.nitrc.org/frs/download.php/10900/mricrogl_windows.zip
- version 1.0.20180328

Source code: dcm2niix

## img2dcm

- img2dcm.exe
- Version 3.6.0 2011-01-06
- Support: https://support.dcmtk.org/docs/img2dcm.html
- Download: https://dicom.offis.de/download/dcmtk/dcmtk360/bin/

Source code: https://github.com/InsightSoftwareConsortium/DCMTK/