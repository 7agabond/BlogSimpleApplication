﻿using BlogSimple.Model.Models;

namespace BlogSimple.Model.ViewModels.BlogViewModels;

public class DashboardIndexViewModel
{
    public IEnumerable<Blog> UserBlogs { get; set; }
}