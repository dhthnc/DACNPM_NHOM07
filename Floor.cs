using System.ComponentModel.DataAnnotations;

namespace DALTW.Models
{
    /// <summary>
    /// Thực thể đại diện cho một Tầng (Floor) trong khách sạn.
    /// Một tầng có thể chứa nhiều phòng.
    /// </summary>
    public class Floor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên tầng không được để trống")]
        [Display(Name = "Tên tầng")]
        public string Name { get; set; }

        /// <summary>
        /// Danh sách các phòng thuộc về tầng này.
        /// </summary>
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
