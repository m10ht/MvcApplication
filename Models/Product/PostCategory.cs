using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product
{
    [Table("ProductCategoryProduct")]
    public class ProductCategoryProduct
    {
        public int ProductId {set; get;}
    
        public int CategoryID {set; get;}
    
        [ForeignKey("ProductId")]
        public ProductModel Product {set; get;}
    
        [ForeignKey("CategoryID")]
        public CategoryProduct Category {set; get;}
    }
}