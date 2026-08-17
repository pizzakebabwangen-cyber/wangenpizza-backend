namespace WangenPizza.Dtos
{
    public class WertgutscheinCheckoutDto
    {
        public decimal FaceValueChf { get; set; }
        public string? Salute { get; set; }
        public string Vorname { get; set; } = "";
        public string Nachname { get; set; } = "";
        public string? Firma { get; set; }
        public string Strasse { get; set; } = "";
        public string Hausnummer { get; set; } = "";
        public string Plz { get; set; } = "";
        public string Ort { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Telefon { get; set; }
        public bool DifferentDelivery { get; set; }
        public string? LieferVorname { get; set; }
        public string? LieferNachname { get; set; }
        public string? LieferStrasse { get; set; }
        public string? LieferHausnummer { get; set; }
        public string? LieferPlz { get; set; }
        public string? LieferOrt { get; set; }
        public string? Bemerkungen { get; set; }
        public int VoucherQuantity { get; set; } = 1;
    }
}
