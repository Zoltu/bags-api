﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Zoltu.BagsMiddleware.Amazon;

namespace Zoltu.BagsMiddleware.Models
{
	public class Product
	{
		[Key]
		public Int32 Id { get; set; }

		[Required]
		public String Name { get; set; }
		[Required]
		public Int64 Price { get; set; }
		public String Asin { get; set; }

		// JSON serialized IEnumerable<Image>
		public String ImagesJson { get; set; }
		public List<ProductPurchaseUrl> PurchaseUrls { get; set; } = new List<ProductPurchaseUrl>();
		public List<ProductTag> Tags { get; set; } = new List<ProductTag>();

		public class Image
		{
			[JsonProperty(PropertyName = "priority")]
			public UInt32 Priority { get; set; }
			[JsonProperty(PropertyName = "small")]
			public String Small { get; set; }
			[JsonProperty(PropertyName = "medium")]
			public String Medium { get; set; }
			[JsonProperty(PropertyName = "large")]
			public String Large { get; set; }
		}

		public dynamic ToBaseWireFormat()
		{
			dynamic result = new ExpandoObject();
			result.id = Id;
			result.name = Name;
			result.price = Price;
			return result;
		}

		public dynamic ToSafeExpandedWireFormat(AmazonUtilities amazon)
		{
			var result = ToBaseWireFormat();
			result.images = JsonConvert.DeserializeObject<IEnumerable<Image>>(ImagesJson ?? "[]");
			result.purchase_urls = new[] { amazon.CreateAssociateLink(Asin) };
			return result;
		}

		public dynamic ToUnsafeExpandedWireFormat(AmazonUtilities amazon)
		{
			var result = ToSafeExpandedWireFormat(amazon);
			result.tags = Tags.Select(productTag => productTag.Tag).Select(tag => tag.ToSafeExpandedWireFormat()).ToList();
			return result;
		}
	}

	public static class ProductExtensions
	{
		public static IQueryable<Product> WithSafeIncludes(this IQueryable<Product> query)
		{
			return query;
		}

		public static IQueryable<Product> WithUnsafeIncludes(this IQueryable<Product> query)
		{
			return query
				.WithSafeIncludes()
				.Include(product => product.Tags).ThenInclude(productTag => productTag.Tag.TagCategory);
		}
	}
}
