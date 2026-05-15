using Pharmacy.DAL;
using Pharmacy.DTO;
using System.Data;

namespace Pharmacy.BLL
{
    public class KhachHangService
    {
        private readonly KhachHangRepositoryDAL _dal =
            new KhachHangRepositoryDAL();

        public DataTable GetDanhSachKhachHang()
        {
            return _dal.GetAll();
        }

        public bool ThemKhachHang(KhachHangDTO kh)
        {
            if (string.IsNullOrWhiteSpace(kh.CCCD))
                return false;

            if (kh.CCCD.Length != 12)
                return false;

            return _dal.Insert(kh);
        }
    }
}