using CAPI.ImageProcessing.Abstraction;

namespace CAPI.ImageProcessing
{
    /// <summary>
    /// Nifti-1 header
    /// </summary>
    public class NiftiHeader : INiftiHeader
    {
        public int sizeof_hdr { get; set; }
        public string dim_info { get; set; }
        public short[] dim { get; set; }
        public float intent_p1 { get; set; }
        public float intent_p2 { get; set; }
        public float intent_p3 { get; set; }
        public short intent_code { get; set; }
        public short datatype { get; set; }
        public short bitpix { get; set; }
        public short slice_start { get; set; }
        public float[] pix_dim { get; set; }
        public float vox_offset { get; set; }
        public float scl_slope { get; set; }
        public float scl_inter { get; set; }
        public short slice_end { get; set; }
        public string slice_code { get; set; }
        public string xyzt_units { get; set; }
        public float cal_max { get; set; }
        public float cal_min { get; set; }
        public float slice_duration { get; set; }
        public float toffset { get; set; }
        public string descrip { get; set; }
        public string aux_file { get; set; }
        public short qform_code { get; set; }
        public short sform_code { get; set; }
        public float quatern_b { get; set; }
        public float quatern_c { get; set; }
        public float quatern_d { get; set; }
        public float qoffset_x { get; set; }
        public float qoffset_y { get; set; }
        public float qoffset_z { get; set; }
        public float[] srow_x { get; set; }
        public float[] srow_y { get; set; }
        public float[] srow_z { get; set; }
        public string intent_name { get; set; }
        public string magic { get; set; }
    }
}