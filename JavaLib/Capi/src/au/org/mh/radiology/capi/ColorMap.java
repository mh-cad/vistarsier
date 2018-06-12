/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package au.org.mh.radiology.capi;

import au.com.nicta.imagej.FalseColor;
import au.com.nicta.imagej.FalseColorSigmoid;
import au.com.nicta.imagej.ImageJUtil;
import au.com.nicta.nifti.NiftiReader;
import static au.org.mh.radiology.capi.MatchNiiWithDicom.MatchStackWithDicoms;
import ij.ImageJ;
import ij.ImagePlus;
import ij.ImageStack;
import ij.io.FileSaver;
import ij.process.ColorProcessor;
import ij.process.ImageProcessor;
import ij.process.ImageStatistics;
import java.awt.image.BufferedImage;
import java.awt.image.DataBufferByte;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.io.FileNotFoundException;
import java.nio.file.FileSystems;
import java.nio.file.Files;
import java.util.Arrays;
import java.util.Properties;
import au.org.mh.radiology.capi.ImgProc;


//Commandline arguments

// Both
// /home/alan/vis_tarsier/vt/preprocess/colormap.config /home/alan/vis_tarsier/temp/flair_new_color_map_dicom /home/alan/vis_tarsier/data/patient_2055832/vt_output/fixed.hdr /home/alan/vis_tarsier/data/patient_2055832/flair_new /home/alan/vis_tarsier/data/patient_2055832/vt_output/intermediate_files/fixed_brain_surface.nii both /home/alan/vis_tarsier/data/patient_2055832/vt_output/structural_changes_dark_in_floating_to_bright_in_fixed.nii /home/alan/vis_tarsier/data/patient_2055832/vt_output/structural_changes_bright_in_floating_to_dark_in_fixed.nii 

// Positive
// /home/alan/vis_tarsier/vt/preprocess/colormap.config /home/alan/vis_tarsier/temp/flair_new_color_map_dicom /home/alan/vis_tarsier/data/patient_2055832/vt_output/fixed.hdr /home/alan/vis_tarsier/data/patient_2055832/flair_new /home/alan/vis_tarsier/data/patient_2055832/vt_output/intermediate_files/fixed_brain_surface.nii positive /home/alan/vis_tarsier/data/patient_2055832/vt_output/structural_changes_dark_in_floating_to_bright_in_fixed.nii

// Negative
// /home/alan/vis_tarsier/vt/preprocess/colormap.config /home/alan/vis_tarsier/temp/flair_new_color_map_dicom /home/alan/vis_tarsier/data/patient_2055832/vt_output/fixed.hdr /home/alan/vis_tarsier/data/patient_2055832/flair_new /home/alan/vis_tarsier/data/patient_2055832/vt_output/intermediate_files/fixed_brain_surface.nii negative /home/alan/vis_tarsier/data/patient_2055832/vt_output/structural_changes_bright_in_floating_to_dark_in_fixed.nii 


/**
 *
 * @author alan
 */
public class ColorMap {
    
    static final boolean VISUALISE = false;
    
    static FalseColorSigmoid _posFalseColor = new FalseColorSigmoid(
            5, // sharpness
            0.35, // centre
            0, 0, 64,
            255, 128, 0 );

    static FalseColorSigmoid _negFalseColor = new FalseColorSigmoid(
            5, // sharpness
            0.35, // centre
            0, 0, 64,
            0, 255, 0 );
    
    static double NORM_NUM_STD_DEV = 2; // defualt, will be overwriten in by the config file


    public static ColorProcessor colorMap(
            BufferedImage background,
            BufferedImage pos,
            BufferedImage neg,
            FalseColor posFalseColor,
            FalseColor negFalseColor ) {
        
        if( pos == null && neg == null ) {
            throw new RuntimeException("Pos and neg can't both be null.");
        }
        
        if( pos != null ) {
            if(    background.getWidth()  != pos.getWidth()
                || background.getHeight() != pos.getHeight() ) {
                throw new RuntimeException( "Background and the overlay color map is different size" );
            }
        }

        if( neg != null ) {
            if(    background.getWidth()  != neg.getWidth()
                || background.getHeight() != neg.getHeight() ) {
                throw new RuntimeException( "Background and the overlay color map is different size" );
            }
        }

//        (new ImagePlus("", background )).show();        
        
        // Unify formats
//        ColorProcessor dst = new ColorProcessor( background );
        ColorProcessor dst = new ColorProcessor( background.getWidth(), background.getHeight() );
        
        final byte[] bgPixels = ((DataBufferByte) background.getRaster().getDataBuffer()).getData();
        final byte[] posPixels = pos == null ? null : ((DataBufferByte) pos.getRaster().getDataBuffer()).getData();
        final byte[] negPixels = neg == null ? null : ((DataBufferByte) neg.getRaster().getDataBuffer()).getData();
        final int[] dstPixels = (int[]) dst.getPixels();
        final int w = background.getWidth();
        final int h = background.getHeight();
        int i = 0;
        for( int y = 0; y < h; ++y ) {
            for( int x = 0; x < w; ++x, ++i ) {
                int srcR = 0;
                int srcG = 0;
                int srcB = 0;
                int gray = 0;

                if( posPixels != null ) {
                    int posGray = posPixels[i] & 0xFF; // & 0xFF converts to unsigned byte
                    if( posGray > 0 ) {
                        srcR = posFalseColor.getR( posGray );
                        srcG = posFalseColor.getG( posGray );
                        srcB = posFalseColor.getB( posGray );
                        gray = posGray;
                    }
                }
                
                if( negPixels != null ) {
                    int negGray = negPixels[i] & 0xFF; // & 0xFF converts to unsigned byte
                    if( negGray > 0 ) {
                        srcR = negFalseColor.getR( negGray );
                        srcG = negFalseColor.getG( negGray );
                        srcB = negFalseColor.getB( negGray );
                        gray = negGray;
                    }
                }
                
                int dstR = bgPixels[i] & 0xFF;
                int dstG = bgPixels[i] & 0xFF;
                int dstB = bgPixels[i] & 0xFF;
                
//                int dstR = (dstPixels[i] >> 16 ) & 0xFF;
//                int dstG = (dstPixels[i] >>  8 ) & 0xFF;
//                int dstB = (dstPixels[i]       ) & 0xFF;

                float alpha = gray / 255.0f;
                dstR = (int)( (1-alpha) * dstR + alpha * srcR );
                dstG = (int)( (1-alpha) * dstG + alpha * srcG );
                dstB = (int)( (1-alpha) * dstB + alpha * srcB );
                
                
                // always opaque
                int p2 = dstR << 16 | dstG << 8 | dstB;

                dstPixels[i] = p2;
            }
        }

        return dst;
    }

    public static void usage() {
        
    }
    
    public static void applyMask( ImagePlus img, ImagePlus mask ) {
        for( int i = 1; i <= img.getNSlices(); ++i ) {
            ImageProcessor srcIp  = img .getStack().getProcessor( i );
            ImageProcessor maskIp = mask.getStack().getProcessor( i );

            for( int y = 0; y < srcIp.getHeight(); ++y ) {
                for( int x = 0; x < srcIp.getWidth(); ++x ) {
                    if( maskIp.get( x, y ) == 0 ) {
                        srcIp.set( x, y, 32768 );
                    }
                }
            }
        }
    }
    

    static FalseColorSigmoid createFalseColorSigmoid( Properties config, String configPrefix ) {
        return new FalseColorSigmoid(
            Float.parseFloat( config.getProperty( configPrefix+"Sharpness" ) ), // sharpness
            Float.parseFloat( config.getProperty( configPrefix+"Centre" ) ), // centre
            Integer.parseInt( config.getProperty( configPrefix+"R1" ) ), // color when value is low
            Integer.parseInt( config.getProperty( configPrefix+"G1" ) ),
            Integer.parseInt( config.getProperty( configPrefix+"B1" ) ),
            Integer.parseInt( config.getProperty( configPrefix+"R2" ) ), // color when value is high
            Integer.parseInt( config.getProperty( configPrefix+"G2" ) ),
            Integer.parseInt( config.getProperty( configPrefix+"B2" ) ) );
    }
    
    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        
        ImageJ ij = null;
        if( VISUALISE ) {
            ij = new ImageJ();
        }

        if( args.length < 3 ) {
//            usage();
            System.out.println( "Arguments supplied: " + Arrays.toString(args) );
            return;
        }

        
        String configFile = args[0];
        String dstDir = args[1];
        String backgroundNiiFile = args[2];
        String backgroundDicomDir = args[3];
        String brainMaskFile = args[4];
        String posFile = args[5];
        String negFile = args[6];        
        String polarityArg = args[7];
        
        System.out.println( "configFile: " + configFile );
        System.out.println( "dstDir: " + dstDir );
        System.out.println( "backgroundNiiFile: " + backgroundNiiFile );
        System.out.println( "backgroundDicomDir: " + backgroundDicomDir );
        System.out.println( "brainMaskFile: " + brainMaskFile );
        System.out.println( "posFile: " + posFile );
        System.out.println( "negFile: " + negFile );
        System.out.println( "polarityArg: " + polarityArg );
        
        int polarity = 0;
        if( polarityArg.compareTo("positive") == 0 ) {
            polarity = 1;
        }
        else if( polarityArg.compareTo("negative") == 0 ) {
            polarity = -1;
        }
        else if( polarityArg.compareTo("both") == 0 ) {
            polarity = 2;
        }
        else {
            usage();
            System.out.println( "Invalid polarity argument" );
            return;
        }
        

        Properties config = new Properties();
        try {
            config.load( new FileReader(configFile) );
        }
        catch( IOException e ) {
            e.printStackTrace();
            return;
        }

        NORM_NUM_STD_DEV = Float.parseFloat( config.getProperty( "normNumStdDev" ) );
        _posFalseColor = createFalseColorSigmoid( config, "pos" );        
        _negFalseColor = createFalseColorSigmoid( config, "neg" );        
        
        
        ImagePlus bg = NiftiReader.load( backgroundNiiFile );
        
        ImagePlus brainMask = NiftiReader.load( brainMaskFile );
        
        ImagePlus pos = (polarity ==  1 || polarity == 2) ? NiftiReader.load( posFile ) : null;
        ImagePlus neg = (polarity == -1 || polarity == 2) ? NiftiReader.load( negFile ) : null;

        // Mehdi Tweaks - Start
        try {
        if (bg != null) bg = ImgProc.Reorient(bg, backgroundDicomDir);
        if (pos != null) pos = ImgProc.Reorient(pos, new int[] { 256,256,160 } );
        if (neg != null) neg = ImgProc.Reorient(neg, new int[] { 256,256,160 } );
        if (brainMask != null) brainMask = ImgProc.Reorient(brainMask, new int[] { 256,256,160 } );
        } catch (FileNotFoundException ex)
        {
            System.out.println(ex.getMessage());
        }
        
        // Mehdi Tweaks - End
        
        ImagePlus bgMasked = bg.duplicate();

        // Apply brain mask
        applyMask( bgMasked, brainMask );
        if( pos != null ) applyMask( pos, brainMask );
        if( neg != null ) applyMask( neg, brainMask );
        
        ImageStatistics brainRegionStats = ImageJUtil.getImageStatistics( bgMasked );
//        ImageStatistics posStats = pos == null ? null : ImageJUtil.getImageStatistics( pos );
//        ImageStatistics negStats = neg == null ? null : ImageJUtil.getImageStatistics( neg );
        
//        bg.setDisplayRange(stats.min, stats.min + stats.stdDev*100);
//        bg.setDisplayRange(stats.min, stats.mgetImax);
        
        System.out.println(brainRegionStats.min + ", " + brainRegionStats.max + ", " + brainRegionStats.stdDev );
//        System.out.println( posStats.min + ", " + posStats.max + ", " + posStats.stdDev );
//        System.out.println( negStats.min + ", " + negStats.max + ", " + negStats.stdDev );
       
//        if( pos != null ) {
//            pos.setDisplayRange( posStats.min, posStats.min + stats.stdDev * NORM_NUM_STD_DEV );
//        }
//        if( neg != null ) {
//            neg.setDisplayRange( negStats.min, negStats.min + stats.stdDev * NORM_NUM_STD_DEV );
//        }
        
        // The difference is already normalised.
        if( pos != null ) {
            pos.setDisplayRange( 0, Short.MAX_VALUE/8*NORM_NUM_STD_DEV );
        }
        if( neg != null ) {
            neg.setDisplayRange( 0, Short.MAX_VALUE/8*NORM_NUM_STD_DEV );
        }

        System.out.println(brainRegionStats.stdDev );
        
        // Reset the display range of the background image. It appears that 
        // the NII conversion has lost some windowing parameters.
        {
            double above = Double.parseDouble( config.getProperty( "bgNumberOfStdDevAboveMean" ) );
            double below = Double.parseDouble( config.getProperty( "bgNumberOfStdDevBelowMean" ) );
                    
            ImageStatistics wholeImageStats = ImageJUtil.getImageStatistics( bg );
            double lo = wholeImageStats.mean - wholeImageStats.stdDev * below;
            double hi = wholeImageStats.mean + wholeImageStats.stdDev * above;
            if( lo < wholeImageStats.min ) lo = wholeImageStats.min;
            if( hi > wholeImageStats.max ) hi = wholeImageStats.max;
            
            bg.setDisplayRange(lo, hi);
        }
                        
        if( VISUALISE ) {
            bg.show("bg");
            if( pos != null ) pos.show("pos");
            if( neg != null ) neg.show("neg");
        }
        
        // Smooth ??  Probably don't dare touch the raw data
//        final double sigma = 0.5;
//        final double accuracy = 0.02; // should not be > 0.02
//        StackProcessor blurr = new StackProcessor() {
//            GaussianBlur blur = new GaussianBlur();
//            @Override public void processSlice(ImageProcessor ip) {
//                blur.blurGaussian( ip, sigma, sigma, accuracy );
//            }
//        };
//        StackProcessor.processStack(pos.getStack(), blurr);
//        StackProcessor.processStack(neg.getStack(), blurr);
//        diffPN = Smooth.smooth(diffPN, false, (float)sigma, false);
//        diffP  = Smooth.smooth(diffP,  false, (float)sigma, false);
        
        
//        new ImageConverter(bg).convertToGray8();

        
//        ImageJUtil.normalize( pos );
//        ImageJUtil.normalize( neg );

//        if( pos.getNSlices() != neg.getNSlices() ) {
//            System.err.println( "Unequal slices in pos and neg: " + pos + ", " + neg );
//            return;
//        }
        
        ImageStack dstStack = new ImageStack( bg.getWidth(), bg.getHeight() );
        
        int slices = pos != null ? pos.getNSlices() : neg.getNSlices();
        
        // Main loop
        for( int i = 1; i <= slices; ++i ) {            
//        for( int i = 80; i <= 80; ++i ) {            
            ImageProcessor bgIP = bg .getStack().getProcessor( i );
            BufferedImage  bgBI = bgIP.getBufferedImage();
            
//            (new ImagePlus("", bgBI )).show();
            
            BufferedImage posBI = pos == null ? null : pos.getStack().getProcessor( i ).getBufferedImage();
            BufferedImage negBI = neg == null ? null : neg.getStack().getProcessor( i ).getBufferedImage();
            
            ColorProcessor res = colorMap( bgBI, posBI, negBI, _posFalseColor, _negFalseColor );
            dstStack.addSlice( "", res );
        }

            
//        bg.show();
        ImagePlus dstPlus = new ImagePlus( "color mapped", dstStack );
        if( VISUALISE ) {
            if( pos != null ) pos.show();
            if( neg != null ) neg.show();
            dstPlus.show();
        }
        
//        for( int i = 1; i <= bg.getNSlices(); ++i ) {
//            bg.getStack().getProcessor(i).flipVertical();
//        }        
                
        try {
            Files.createDirectory( FileSystems.getDefault().getPath(dstDir) );
        }
        catch( IOException e ) {
            e.printStackTrace();
        }
        
//        NiftiWriter.save( dstPlus, dstNiiFile );
        
        File[] matchingNames = MatchStackWithDicoms( backgroundDicomDir, bg );
        
        // Write as BMP
        int test = dstPlus.getNSlices();
        for( int i = 1; i <= dstPlus.getNSlices(); ++i ) {
            String s = matchingNames[i-1].getName();
            String name = String.format("%s/%s.bmp", dstDir, s);
            dstPlus.setSlice(i);
            //dstPlus.getProcessor().flipVertical();
            System.out.println("Saving: " + name);
            (new FileSaver(dstPlus)).saveAsBmp(name);
        }

        // Write as DICOM
//        for( int i = 1; i <= dstPlus.getNSlices(); ++i ) {
//            String s = matchingNames[i-1].getName().replaceFirst("[.][^.]+$", "");
//            String name = String.format("%s/%s.dcm", dstDir, s);
//            dstPlus.setSlice(i);
//            dstPlus.getProcessor().flipVertical();
////            (new FileSaver(dstPlus)).saveAsBmp(name);
//            
//            try {
//                ImagePlus plus = new ImagePlus( "", dstPlus.getProcessor() );
//                DicomExporter exporter = new DicomExporter();
//                DicomObject header = exporter.createHeader(plus, true, true, true);
//                exporter.write(header, plus, new File(name), false);                    
//            }
//            catch( IOException e ) {
//                e.printStackTrace();
//            }
//            
//        }
        
        System.out.println("Completed");
    }

}
