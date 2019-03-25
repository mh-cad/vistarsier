﻿using CAPI.ImageProcessing.Abstraction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPI.ImageProcessing
{
    public class Registration
    {
        public static INifti CMTKRegistration(INifti floating, INifti reference, DataReceivedEventHandler updates = null)
        {
            // Setup our temp file names.
            string niftiInPath = Tools.TEMPDIR + floating.GetHashCode() + ".cmtkrego.in.nii";
            string niftiRefPath = Tools.TEMPDIR + floating.GetHashCode() + ".cmtkrego.ref.nii";
            string niftiOutPath = Tools.TEMPDIR + floating.GetHashCode() + ".cmtkrego.out.nii";
            string regOutPath = Tools.TEMPDIR + floating.GetHashCode() + ".cmtkrego.reg";

            // Write nifti to temp directory.
            floating.WriteNifti(niftiInPath);
            reference.WriteNifti(niftiRefPath);

            Environment.SetEnvironmentVariable("CMTK_WRITE_UNCOMPRESSED", "1");

            var args = $"-o {regOutPath} {niftiRefPath} {niftiInPath}";
            Tools.ExecProcess("../../../ThirdPartyTools/CMTK/registration.exe", args, updates);

            args = $"-o {niftiOutPath} --floating {niftiInPath} {niftiRefPath} {regOutPath}";
            Tools.ExecProcess("../../../ThirdPartyTools/CMTK/reformatx.exe", args, updates);

            //INifti output = floating.DeepCopy();
            floating.ReadNifti(niftiOutPath);

            return floating;
        }
    }
}
