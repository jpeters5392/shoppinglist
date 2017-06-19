﻿using System;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using System.Diagnostics;
using Xamarin.Forms;
using System.IO;
using Plugin.Connectivity;
using Splat;
using shoppinglist.Models;
using System.Linq;

namespace shoppinglist.Services
{
	public class CategoryService : AzureService<Category>
	{
		protected override IMobileServiceSyncTable<Category> Table
		{
			get
			{
                return this.SqlInitializer.CategoryTable;
			}
		}

		public CategoryService() : base("allCategories")
		{

		}

		public async Task<IEnumerable<Category>> GetCategories()
		{
			var items = await GetItems(x => true);

			return items.OrderBy(c => c.Name);
		}

		public async Task<Category> AddCategory(string name)
		{
			var item = new Category
			{
				Name = name
			};

			return await AddItem(item);
		}
	}
}