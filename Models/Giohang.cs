using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BASIC_PROJECT.Models
{
    public class Giohang
    {
        NhaHangEntities3 db = new NhaHangEntities3();
        public int MaMONAN { get; set; }
        public string TenMonan { get; set; }        
        public double DonGia { get; set; }

        public int Soluong { get; set; }
        public double Thanhtien
        {
            get { return Soluong * DonGia; }
        }

        public Giohang(int ms)
        {
            MaMONAN = ms;
            MonAn s = db.MonAns.Single(n => n.MaMA == MaMONAN);
            TenMonan = s.TenMon;        
            DonGia = double.Parse(s.Gia.ToString());
            Soluong = 1;
        }
    }
}