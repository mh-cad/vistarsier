/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package au.org.mh.radiology.capi;

import au.com.nicta.nifti.NiftiReader;
import ij.ImageJ;
import ij.ImagePlus;
import ij.ImageStack;
import ij.process.ImageProcessor;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.Arrays;
import lu.tudor.santec.dicom.DicomOpener;
import lu.tudor.santec.dicom.exporter.DicomExporter;
import org.dcm4che2.data.DicomObject;
import au.org.mh.radiology.capi.ImgProc;

// Command line arguments

// /home/alan/vis_tarsier/data/patient_2055832/vt_output/fixed.img /home/alan/vis_tarsier/data/patient_2055832/flair_new /home/alan/vis_tarsier/data/patient_2055832/vt_output/intermediate_files/floating_resliced.nii /home/alan/vis_tarsier/temp/flair_old_resliced_dicom

/**
 *
 * @author alan
 */
public class MatchNiiWithDicom {

    static final boolean VISUALISE = false;

    static boolean isMatch( ImageProcessor a, ImageProcessor b ) {
        if(    a.getWidth()  != b.getWidth()
            || a.getHeight() != b.getHeight() ) {
            throw new RuntimeException("Image size does not match.");
        }
        
        int xStep = a.getWidth()  / 10;
        int yStep = a.getHeight() / 10;
                      
        // Sparse test
        for( int y = yStep/2; y < a.getHeight(); y += yStep ) {            
            for( int x = xStep/2; x < a.getWidth(); x += xStep ) {
                int aa = a.getPixel(x, y);
                int bb = b.getPixel(x, y);
                
                if( aa >= 32768 ) aa -= 32768;
                if( bb >= 32768 ) bb -= 32768;
                
                if( aa != bb ) {
                    return false;
                }
            }
        }
        
        // Complete image test
        for( int y = 0; y < a.getHeight(); ++y ) {
            for( int x = 0; x < a.getWidth(); ++x ) {
                int aa = a.getPixel(x, y);
                int bb = b.getPixel(x, y);
                if( aa >= 32768 ) aa -= 32768;
                if( bb >= 32768 ) bb -= 32768;
                                        
                if( aa != bb ) {
                    return false;
                }
            }
        }
        
        return true;
    }

    
    static File[] MatchStackWithDicoms( String dicomDir, ImagePlus stack ) {
                
        File[] files = new File(dicomDir).listFiles();
        int numMatches = 0;
        File[] ret = new File[stack.getNSlices()];
        for( File file : files ) {
//            if( 0 != file.getName().compareTo( "1.3.12.2.1107.5.2.32.35208.2015011520220242625069081.dcm" ) ) {
//                continue;
//            }
            
            ImagePlus dicom = null;
            try {
                dicom = DicomOpener.loadImage(file);
            }
            catch(FileNotFoundException e) {
                throw new RuntimeException(e);
            }
            if( dicom.getNSlices() != 1 ) {
                throw new RuntimeException("Multi frame dicom is not supported.");
            }
            ImageProcessor dicomIp = dicom.getStack().getProcessor( 1 ); // expecting a single frame
            
            int i;
            for( i = 1; i <= stack.getNSlices(); ++i ) {            
//            for( i = 160; i <= 160; ++i ) {            
                ImageProcessor stackIp = stack.getStack().getProcessor( i );
                ImageProcessor
                stackIpFlipped = stackIp.duplicate();
                stackIpFlipped.flipVertical();
                
                /// Mehdi Tweaks - Start
                stackIp = stackIp.convertToShort(false);
                /// Mehdi Tweaks - End
                                                
                if(    isMatch(dicomIp, stackIp)
                    || isMatch(dicomIp, stackIpFlipped) ) {
                    ret[i-1] = file;
                    ++numMatches;
                    System.out.println("Matching: " + file.getName());
                    break;
                }
            }
            if( i > stack.getNSlices() ) {
                System.out.println("No match for: " + file.getName());
            }
//            break;
        }
        if( numMatches != stack.getNSlices() ) {
            throw new RuntimeException("There are unmatched slices.");
        }
        
        return ret;
    }

    public static void main(String[] args) {
        
        ImageJ ij = null;
        if( VISUALISE ) {
            ij = new ImageJ();
        }
        
        // Use refNii and refDicom to establish correspondence beyween nii slices
        // and DICOM files. Then assume the same order of srcNii to refNii and then
        // dump as bitmap files into dstDicomDir.
        final String refNiiFile = args[0];
        final String refDicomDir = args[1];
        final String srcNiiFile = args[2];
        final String srcBmpDir = args[3];
                
        ImagePlus refNii = NiftiReader.load( refNiiFile );
        ImagePlus srcNii = NiftiReader.load( srcNiiFile );        
        
        File[] matchingNames = MatchStackWithDicoms( refDicomDir, refNii );
        
        System.out.println( Arrays.toString(matchingNames) );
        
        for( int i = 1; i <= srcNii.getNSlices(); ++i ) {
//        for( int i = 1; i <= 1; ++i ) {
            String s = matchingNames[i-1].getName();
            String name = String.format("%s/%s", srcBmpDir, s);
            srcNii.setSlice(i);
            srcNii.getProcessor().flipVertical();
//            (new FileSaver(srcNii)).saveAsTiff(name);

            try {
                ImagePlus plus = new ImagePlus( "", srcNii.getProcessor() );
                DicomExporter exporter = new DicomExporter();
                DicomObject header = exporter.createHeader(plus, true, true, true);
                exporter.write(header, plus, new File(name), false);
            }
            catch( IOException e ) {
                e.printStackTrace();
            }
        }
        
        System.out.println( "Completed" );
    }
    
    
}
