namespace VisTarsier.NiftiLib
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

        public INiftiHeader DeepCopy()
        {
            NiftiHeader copy = new NiftiHeader();

            copy.sizeof_hdr = sizeof_hdr;
            copy.dim_info = dim_info;
            copy.dim = new short[dim.Length];
            dim.CopyTo(copy.dim, 0);
            copy.intent_p1 = intent_p1;
            copy.intent_p2 = intent_p2;
            copy.intent_p3 = intent_p3;
            copy.intent_code = intent_code;
            copy.datatype = datatype;
            copy.bitpix = bitpix;
            copy.slice_start = slice_start;
            copy.pix_dim = new float[pix_dim.Length];
            pix_dim.CopyTo(copy.pix_dim, 0);
            copy.vox_offset = vox_offset;
            copy.scl_slope = scl_slope;
            copy.scl_inter = scl_inter;
            copy.slice_end = slice_end;
            copy.slice_code = slice_code;
            copy.xyzt_units = xyzt_units;
            copy.cal_max = cal_max;
            copy.cal_min = cal_min;
            copy.slice_duration = slice_duration;
            copy.toffset = toffset;
            copy.descrip = descrip;
            copy.aux_file = aux_file;
            copy.qform_code = qform_code;
            copy.sform_code = sform_code;
            copy.quatern_b = quatern_b;
            copy.quatern_c = quatern_c;
            copy.quatern_d = quatern_d;
            copy.qoffset_x = qoffset_x;
            copy.qoffset_y = qoffset_y;
            copy.qoffset_z = qoffset_z;
            copy.srow_x = new float[srow_x.Length];
            srow_x.CopyTo(copy.srow_x, 0);
            copy.srow_y = new float[srow_y.Length];
            srow_y.CopyTo(copy.srow_y, 0);
            copy.srow_z = new float[srow_z.Length];
            srow_z.CopyTo(copy.srow_z, 0);
            copy.intent_name = intent_name;
            copy.magic = magic;

            return copy;
        }
    }
}