﻿using BlogSimple.Model.ViewModels.BlogViewModels;
using BlogSimple.Model.ViewModels.HomeViewModels;
using BlogSimple.Web.BusinessManager.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogSimple.Web.Controllers;

public class HomeController : Controller
{
    private readonly IHomeBusinessManager _homeBusinessManager;

    public HomeController(IHomeBusinessManager homeBusinessManager)
    {
        _homeBusinessManager = homeBusinessManager;
    }

    public async Task<IActionResult> Index(string searchString)
    {
        HomeIndexViewModel homeIndexViewModal = await _homeBusinessManager.GetHomeIndexViewModel(searchString);

        return View(homeIndexViewModal);
    }

    // GET: HomeController/Details/Id
    public async Task<ActionResult> Details(string id)
    {
        BlogDetailsViewModel viewModel = await _homeBusinessManager.GetHomeDetailsViewModel(id);

        return View(viewModel);
    }
}