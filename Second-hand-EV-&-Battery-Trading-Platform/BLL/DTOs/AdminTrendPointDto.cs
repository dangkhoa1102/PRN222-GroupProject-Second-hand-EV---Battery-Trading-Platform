namespace BLL.DTOs;

public class AdminTrendPointDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }

    public string Label => $"{Month:00}/{Year}";
}


