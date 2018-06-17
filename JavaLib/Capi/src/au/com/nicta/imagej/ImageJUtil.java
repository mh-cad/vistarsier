/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package au.com.nicta.imagej;

import ij.ImagePlus;
import ij.ImageStack;
import ij.io.FileInfo;
import ij.process.ByteProcessor;
import ij.process.ColorProcessor;
import ij.process.FloatProcessor;
import ij.process.ImageProcessor;
import ij.process.ImageStatistics;
import ij.process.ShortProcessor;
import java.util.Map;

/**
 *
 * @author davidjr
 */
public class ImageJUtil {

    public static int getPixelValueRange( ImageProcessor ip ) {
        if( ip instanceof ByteProcessor  ) return 256;
        if( ip instanceof ShortProcessor ) return 65536;
        if( ip instanceof FloatProcessor  ) return 1;
        if( ip instanceof ColorProcessor  ) return 256;
        return 0;
    }
    
    public static double getZScale( ImagePlus ip ) {
        FileInfo fi = ip.getFileInfo();
        double xPxSize = fi.pixelWidth;
        double zPxSize = fi.pixelDepth;
        double zPxScale = zPxSize / xPxSize;
        return zPxScale;
    }
    
    public static void normalize( ImagePlus ip ) {
        ImageStatistics stats = ImageJUtil.getImageStatistics( ip );

//        System.out.println( "stack stats.mean: " + stats.mean );
//        System.out.println( "stack stats.stdDev: " + stats.stdDev );
        normalize( ip, stats.mean, stats.stdDev );
    }

    public static void normalize( ImagePlus src, double mean, double stdDev ) {
        for( int i = 1; i <= src.getNSlices(); ++i ) {
            normalize( src.getStack().getProcessor(i), mean, stdDev );
        }
    }

    public static void normalize( ImageProcessor ip ) {
        int statOps =   ImageStatistics.MEAN
                      + ImageStatistics.STD_DEV;

        ImageStatistics stats = ImageStatistics.getStatistics( ip, statOps, null );

        double mean = stats.mean;
        double stdDev = stats.stdDev;

        normalize( ip, mean, stdDev );
    }

    public static void normalize( ImageProcessor ip, double mean, double stdDev ) {
        float[] ps = (float[]) ip.getPixels();
        for( int j = 0; j < ps.length; ++j ) {
            ps[j] = (float)( ((double)ps[j] - mean) / (stdDev) * (Short.MAX_VALUE/8) + (Short.MAX_VALUE/2) );
        }
    }

    public static ImageStatistics getImageStatistics( ImagePlus ip ) {
        return getImageStatistics( ip, 1, ip.getNSlices() );
    }

    public static ImageStatistics getImageStatistics( ImagePlus ip, int firstSlice, int lastSlice ) {
        double mean = 0;
        double var = 0;
        double min = Double.POSITIVE_INFINITY;
        double max = Double.NEGATIVE_INFINITY;
        
        for( int i = firstSlice; i <= lastSlice; ++i )
        {
            int statOps =   ImageStatistics.MEAN
                          + ImageStatistics.STD_DEV
                          + ImageStatistics.MIN_MAX;
            ImageStatistics stats = ImageStatistics.getStatistics( ip.getStack().getProcessor(i), statOps, null );

            mean += stats.mean;
            var  += stats.stdDev * stats.stdDev; // variance, will be converted below
            min = Math.min(min, stats.min);
            max = Math.max(max, stats.max);
        }
        ImageStatistics stats = new ImageStatistics();
        int sliceCnt = lastSlice - firstSlice + 1;
        stats.mean = mean / sliceCnt;
        stats.stdDev = Math.sqrt( var / sliceCnt );
        stats.min = min;
        stats.max = max;
        return stats;
    }

    public static class StatsResult {
        public double _sum = 0;
        public double _sqSum = 0;
        public int _cnt = 0;
    }

//    static StatsResult getSums( float[] data, byte[] mask ) {
//        double sum = 0;
//        double sqSum  = 0;
//        int cnt = 0;
//        for( int i = 0; i < data.length; ++i ) {
//            if( mask[i] != 0 ) {
//                float v = data[i];
//                sum += v;
//                sqSum += v*v;
//                ++cnt;
//            }
//        }
//
//        StatsResult ret = new StatsResult();
//        ret._sum = sum;
//        ret._sqSum = sqSum;
//        ret._cnt = cnt;
//        return ret;
//    }
//
//    static StatsResult getSums( short[] data, byte[] mask ) {
//        double sum = 0;
//        double sqSum  = 0;
//        int cnt = 0;
//        for( int i = 0; i < data.length; ++i ) {
//            if( mask[i] != 0 ) {
//                short v = data[i];
//                sum += v;
//                sqSum += v*v;
//                ++cnt;
//            }
//        }
//
//        StatsResult ret = new StatsResult();
//        ret._sum = sum;
//        ret._sqSum = sqSum;
//        ret._cnt = cnt;
//        return ret;
//    }


    public static StatsResult getSums( ImageProcessor ip, ImageProcessor mask ) {
        double sum = 0;
        double sqSum  = 0;
        int cnt = 0;
        for( int i = 0; i < ip.getPixelCount(); ++i ) {
            if( mask.get(i) != 0 ) {
                double v = ip.getf(i);
                sum += v;
                sqSum += v*v;
                ++cnt;
            }
        }
        
        StatsResult ret = new StatsResult();
        ret._sum = sum;
        ret._sqSum = sqSum;
        ret._cnt = cnt;
        return ret;
    }

    public static ImageStatistics getMeanStdDev( ImageProcessor ip, ImageProcessor mask ) {

        StatsResult sr = getSums( ip, mask );

        double sum   = sr._sum;
        double sqSum = sr._sqSum;
        int    cnt   = sr._cnt;

        ImageStatistics ret = new ImageStatistics();
        if( cnt != 0 ) {
            ret.mean   = sum/cnt;
            ret.stdDev = Math.sqrt( Math.max( 0, sqSum/cnt - ret.mean*ret.mean ) );
        }
        else {
            ret.mean   = 0;
            ret.stdDev = 1;
        }
        return ret;
    }

    public static ImageStatistics getMeanStdDev( ImagePlus ip, ImagePlus mask ) {
        return getMeanStdDev( ip, mask, 1, ip.getNSlices() );
    }

    public static ImageStatistics getMeanStdDev( ImagePlus ip, ImagePlus mask, int firstSlice, int lastSlice ) {
        double sum = 0;
        double sqSum = 0;
        int cnt = 0;

        for( int i = firstSlice; i <= lastSlice; ++i )
        {
            StatsResult sr = getSums( ip.getStack().getProcessor(i), mask.getStack().getProcessor(i) );

            sum   += sr._sum;
            sqSum += sr._sqSum;
            cnt   += sr._cnt;
        }

        ImageStatistics ret = new ImageStatistics();
        if( cnt != 0 ) {
            ret.mean   = sum/cnt;
            ret.stdDev = Math.sqrt( Math.max( 0, sqSum/cnt - ret.mean*ret.mean ) );
        }
        else {
            ret.mean   = 0;
            ret.stdDev = 1;
        }
        return ret;
    }

    public static ImagePlus getDiffLoHi(
        ImagePlus src1,
        ImagePlus src2,
        ImagePlus mask,
        int firstSliceIdx,
        int  lastSliceIdx,
        double max ) {
        return getDiff( src1, src2, mask, firstSliceIdx, lastSliceIdx, max, true );
    }

    static public ImagePlus getDiffHiLo(
        ImagePlus src1,
        ImagePlus src2,
        ImagePlus mask,
        int firstSliceIdx, 
        int  lastSliceIdx,
        double max ) {
        return getDiff( src1, src2, mask, firstSliceIdx, lastSliceIdx, max, false );
    }

    static public ImagePlus getDiff(
        ImagePlus src1,
        ImagePlus src2,
        ImagePlus mask,
        int firstSliceIdx, 
        int  lastSliceIdx,
        double max,
        boolean darkToBright ) {

        ImageStack stack = new ImageStack( src1.getWidth(), src1.getHeight() );

        // the edges are not registered very well, lots of noise
        for( int i = firstSliceIdx; i <= lastSliceIdx; ++i ) {

            FloatProcessor dst = new FloatProcessor( src1.getWidth(), src1.getHeight() );
            float[] data1    = (float[]) src1.getStack().getProcessor(i).getPixels();
            float[] data2    = (float[]) src2.getStack().getProcessor(i).getPixels();
            byte[] dataMask = (byte[]) mask.getStack().getProcessor(i).getPixels();
            float[] dstData = (float[]) dst.getPixels();

            for( int j = 0; j < data1.length; ++j ) {
                if( dataMask[j] == 0 )
                {
                    continue;
                }
                int a = ((int)data1[j]) & 0x0000ffff;
                int b = ((int)data2[j]) & 0x0000ffff;
                if( darkToBright ) {
                    if( b > a ) {
                        dstData[j] = (float)( b - a ); // a is darker, b is brighter, hence darkToBright
                    }
                    else {
                        dstData[j] = 0;
                    }
                }
                else {
                    if( a > b ) {
                        dstData[j] = (float)( a - b );
                    }
                    else {
                        dstData[j] = 0;
                    }
                }

                dst.setMinAndMax(0, max);
            }

            stack.addSlice(null, dst);
        }

        if( darkToBright ) {
            return new ImagePlus( "Dark to BrightDifference", stack );
        }
        else{
            return new ImagePlus( "Bright to Dark Difference", stack );
        }
    }

    public static void copyProperties( ImagePlus from, ImagePlus into ) {
        if( from.getProperties() != null ) {
            for( Map.Entry<Object, Object> e : from.getProperties().entrySet() ){
                into.setProperty( (String)e.getKey(), e.getValue() );
            }
        }
    }

}
