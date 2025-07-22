using System.Collections.Generic;
using FigureShop.Models;

namespace FigureShop.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Product> Products { get; set; } = new List<Product>();
    }
} 