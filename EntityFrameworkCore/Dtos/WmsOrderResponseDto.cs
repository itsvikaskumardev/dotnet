namespace EntityFrameworkCore.Dtos
{
    public class WmsOrderResponseDto
    {
        public Guid Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public List<WmsOrderItemResponseDto> Items { get; set; }
            = new();

        public decimal OrderTotal { get; set; }
    }
}
