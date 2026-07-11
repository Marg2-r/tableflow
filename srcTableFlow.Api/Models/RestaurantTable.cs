namespace TableFlow.Api.Models
{
    public class RestaurantTable
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;  

        public int Capacity { get; set; }

        public string Zone { get; set; } = string.Empty;

        public double XPosition { get; set; }
        public double YPosition { get; set; }

        public bool IsActive { get; set; }

    }
}
