/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package au.org.mh.radiology.capi;

import ij.ImagePlus;
import ij.ImageStack;
import java.io.File;
import java.io.FileNotFoundException;
import lu.tudor.santec.dicom.DicomOpener;

/**
 *
 * @author Mehdi
 */
public class ImgProc {
    public static ImagePlus Reorient(ImagePlus imgPlus, int[] dimTarget) {//throws Exception {
        int slices = imgPlus.getNSlices();
        int width = imgPlus.getWidth();
        int height = imgPlus.getHeight();
        int tw = dimTarget[0];
        int th = dimTarget[1];
        int ts = dimTarget[2];
        //if (!(slices*width*height == tw*th*ts)) throw new Exception("Dimensions do not match");
        System.out.println();
        System.out.println("Reorient:");
        System.out.println("Source Slices: " + slices);
        System.out.println("Source Width: " + width);
        System.out.println("Source Height: " + height);
        
        float[] allPixels = GetAllPixels(imgPlus);
        float[] newPixels = new float[allPixels.length];
        float min = Min(allPixels);
        
        for (int z = 0; z < ts; z++) {
            for (int y = 0; y < th; y++) {
                for (int x = 0; x < tw; x++) {
                    //int op = (ts-1-z) + (x*ts) + ((th-1-y)*ts*tw); // flipped
                    //int op = ts - 1 - z + ((tw-1-x)*ts) + ((th-1-y)*ts*tw); // Correct
                    int op = z + ((tw-1-x)*ts) + ((th-1-y)*ts*tw); // wrong side
                    int np= x + (y*tw) + (z*tw*th);
                    newPixels[np] = allPixels[op] - min; // scale in a way that minimum value will be zero similar to dicom ImageProcessor
                }
            }
        }
        
        ImageStack is = new ImageStack(tw,th);
        for (int z = 0; z < ts; z++) {
            int src = z*th*tw;
            int dst = (z+1)*th*tw;
            float[] slice = java.util.Arrays.copyOfRange(newPixels, src, dst);
            is.addSlice(Integer.toString(z), slice);
        }
        ImagePlus ro = new ImagePlus("Reoriented", is);
        ro.setDimensions(tw, th, ts);
        
        System.out.println();
        System.out.println("Target Slices: " + ro.getNSlices());
        System.out.println("Target Width: " + ro.getWidth());
        System.out.println("Target Height: " + ro.getHeight());
        
        //ro.show("Reoriented Stack");
        
        return ro;
    }
    
    public static ImagePlus Reorient(ImagePlus imgPlus, String dicomDir) throws FileNotFoundException {
        if (!new File(dicomDir).exists()) throw new FileNotFoundException("Could not find following folder:" + dicomDir);
        File[] dcmFiles = new File(dicomDir).listFiles();
        if (dcmFiles.length  == 0) throw new FileNotFoundException("Following folder contains no files:" + dicomDir);
        
        ImagePlus firstDcm = DicomOpener.loadImage(dcmFiles[0]);
                
        int width = firstDcm.getWidth();
        int height = firstDcm.getHeight();
        int slices = dcmFiles.length;
        
        return Reorient(imgPlus, new int[] {width, height, slices });
    }
    
    private static float[] GetAllPixels(ImagePlus imgPlus){
        int pixelsLength = imgPlus.getNSlices() * imgPlus.getProcessor().getWidth() * imgPlus.getProcessor().getHeight();
        float[] allPixels = new float[pixelsLength];
        int slices = imgPlus.getNSlices();
        for (int s = 1; s <= slices; s++) {
            imgPlus.setSlice(s);
            Object dataType = imgPlus.getProcessor().getPixels().getClass();
            if (dataType.toString().contains("class [S")) {
                short[] pixels = (short[]) imgPlus.getProcessor().getPixels();
                for (int p = 0; p < pixels.length; p++) {
                    int offset = (s-1) * pixels.length;
                    allPixels[offset + p] = pixels[p];
                }
            }
            if (dataType.toString().contains("class [B")) {
                byte[] pixels = (byte[]) imgPlus.getProcessor().getPixels();
                int max = Max(pixels);
                for (int p = 0; p < pixels.length; p++) {
                    int offset = (s-1) * pixels.length;
                    if (max == 0) allPixels[offset + p] = Math.abs(pixels[p]);
                    else allPixels[offset + p] = Math.abs(pixels[p]);
                }
            }
            if (dataType.toString().contains("class [F")) {
                float[] pixels = (float[]) imgPlus.getProcessor().getPixels();
                for (int p = 0; p < pixels.length; p++) {
                    int offset = (s-1) * pixels.length;
                    allPixels[offset + p] = (short)Math.round(pixels[p]);
                }
            }
        }
         return allPixels;
    }
        
    private static byte Max(byte[] arr){
        byte max = Byte.MIN_VALUE;
        for (int i = 0; i < arr.length; i++) {
            if (max<arr[i]) max = arr[i];
        }
        return max;
    }
    private static float Min(float[] arr){
        float min = Float.MAX_VALUE;
        for (int i = 0; i < arr.length; i++) {
            if (min > arr[i]) min = arr[i];
        }
        return min;
    }
}