/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package au.org.mh.radiology.capi;

import au.com.nicta.nifti.NiftiIO;
import ij.ImagePlus;
import lu.tudor.santec.dicom.DicomOpener;
import java.io.File;
import java.io.FileNotFoundException;
import ij.process.ImageProcessor;
import java.nio.file.Files;
import java.io.IOException;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.LinkOption;
import java.lang.Object;

/**
 *
 * @author Mehdi
 */
public class NictaTest {
    /**
     * @param args the command line arguments
     */
    private static void main(String[] args) {
        String configFile = "D:/Capi-Tests/colormap.config";
        String dstDir = "D:/Capi-Tests/TestsResources/MF-PC/Output";
        String backgroundNiiFile = "D:/Capi-Tests/TestsResources/MF-PC/Fixed/fixed.nii";
        String backgroundDicomDir = "D:/Capi-Tests/TestsResources/MF-PC/Fixed/Dicom";
        String brainMaskFile = "D:/Capi-Tests/TestsResources/MF-PC/Fixed/fixed.mask.nii";
        String posFile = "D:/Capi-Tests/TestsResources/MF-PC/sub.pos.nii";
        String negFile = "D:/Capi-Tests/TestsResources/MF-PC/sub.neg.nii";        
        String polarityArg = "negative";
        
        Path path = Paths.get(dstDir);
        try{
            Files.deleteIfExists(path);
        }
        catch(IOException ex){}
        
        ColorMap.main(new String[] {configFile, dstDir, backgroundNiiFile, backgroundDicomDir, brainMaskFile, posFile, negFile, polarityArg});
    }
}