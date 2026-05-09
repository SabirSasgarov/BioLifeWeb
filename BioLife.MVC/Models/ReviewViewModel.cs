using BioLife.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace BioLife.MVC.Models
{
	public class ReviewViewModel
	{
		public string UserName { get; set; }
		public string Comment { get; set; }
		public decimal Rating { get; set; }
		public DateTime CreatedDate { get; set; }
	}
	public class AddReviewViewModel
	{
		[Required, StringLength(500, MinimumLength = 3,ErrorMessage = "Comment must be between 3 and 500 characters.")]
		public string Comment { get; set; }
		[Required]
		[Range(0.5, 5, ErrorMessage = "Rating must be between 0.5 and 5.")]
		public decimal Rating { get; set; }
	}
	public class ProductDetailViewModel
	{
		public Product Product { get; set; } = null!;
		public List<ReviewViewModel> Reviews { get; set; } = [];
		public AddReviewViewModel NewReview { get; set; } = new();
	}

}
