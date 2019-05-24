namespace VisTarsier.NiftiLib
{
    public interface INiftiHeader
    {
#pragma warning disable IDE1006 // Naming Styles
        int sizeof_hdr { get; set; }
        string dim_info { get; set; }
        short[] dim { get; set; }
        float intent_p1 { get; set; }
        float intent_p2 { get; set; }
        float intent_p3 { get; set; }
        short intent_code { get; set; }
        short datatype { get; set; }
        short bitpix { get; set; }
        short slice_start { get; set; }
        float[] pix_dim { get; set; }
        float vox_offset { get; set; }
        float scl_slope { get; set; }
        float scl_inter { get; set; }
        short slice_end { get; set; }
        string slice_code { get; set; }
        string xyzt_units { get; set; }
        float cal_max { get; set; }
        float cal_min { get; set; }
        float slice_duration { get; set; }
        float toffset { get; set; }
        string descrip { get; set; }
        string aux_file { get; set; }
        short qform_code { get; set; }
        short sform_code { get; set; }
        float quatern_b { get; set; }
        float quatern_c { get; set; }
        float quatern_d { get; set; }
        float qoffset_x { get; set; }
        float qoffset_y { get; set; }
        float qoffset_z { get; set; }
        float[] srow_x { get; set; }
        float[] srow_y { get; set; }
        float[] srow_z { get; set; }
        string intent_name { get; set; }
        string magic { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        INiftiHeader DeepCopy();
    }
}