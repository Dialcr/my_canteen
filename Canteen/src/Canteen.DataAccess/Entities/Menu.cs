namespace Canteen.DataAccess.Entities;

public class Menu
{
        [Key]
        public int Id { get; set; }

        public DateTimeOffset Date { get; set; }

        [Required]
        public int EstablishmentId { get; set; }

        [ForeignKey(nameof(EstablishmentId))]
        public Establishment? Establishment { get; set; }

        public ICollection<MenuProduct>? MenuProducts { get; set; }
}