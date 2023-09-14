﻿using BlogSimple.Model.Models;
using BlogSimple.Model.Services.Interfaces;
using BlogSimple.Model.ViewModels.PostViewModels;
using BlogSimple.Web.BusinessManager.Interfaces;
using BlogSimple.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace BlogSimple.Web.BusinessManager;

public class PostBusinessManager : IPostBusinessManager
{
    private readonly UserManager<User> _userManager;
    private readonly IPostService _postService;
    private readonly IUserService _userService;
    private readonly ICommentService _commentService;
    private readonly ICommentReplyService _commentReplyService;
    private readonly IWebHostEnvironment webHostEnvironment;

    public PostBusinessManager(
        UserManager<User> userManager,
        IPostService postService,
        IUserService userService,
        ICommentService commentService,
        ICommentReplyService commentReplyService,
            IWebHostEnvironment webHostEnvironment
        )
    {
        _userManager = userManager;
        _postService = postService;
        _userService = userService;
        _commentService = commentService;
        _commentReplyService = commentReplyService;
        this.webHostEnvironment = webHostEnvironment;
    }

    public async Task<DashboardIndexViewModel> GetDashboardIndexViewModel(string searchString, ClaimsPrincipal claimsPrincipal)
    {
        IEnumerable<Post> posts = await _postService.GetAll(searchString ?? string.Empty);

        var user = await _userManager.GetUserAsync(claimsPrincipal);

        var loggedInUser = await _userService.Get(user.Id);

        var userPosts = posts.Where(b => b.CreatedBy.Email == user.Email);

        return new DashboardIndexViewModel
        {
            UserPosts = userPosts,
            AccountUser = loggedInUser,
        };
    }

    public async Task<FavoritePostsViewModel> GetFavoritePostsViewModel(string searchString, ClaimsPrincipal claimsPrincipal)
    {
        var user = await _userManager.GetUserAsync(claimsPrincipal);

        var usersFavoritePosts = user.FavoritedPosts.ToList();

        return new FavoritePostsViewModel
        {
            UsersFavoritePosts = usersFavoritePosts,
            AccountUser = user
        };
    }

    public async Task<PostDetailsViewModel> FavoritePost(string id, ClaimsPrincipal claimsPrincipal)
    {
        var user = await _userManager.GetUserAsync(claimsPrincipal);

        var post = await _postService.Get(id);

        var userAlreadyFavorited = user.FavoritedPosts.Where(b => b.Id == post.Id).FirstOrDefault();

        if (userAlreadyFavorited is null)
        {
            user.FavoritedPosts.Add(post);
        }
        else
        {
            var removePost = user.FavoritedPosts.Where(b => b.Id == post.Id).FirstOrDefault();
            user.FavoritedPosts.Remove(removePost);
        }
        var savedUser = await _userService.Update(user.UserName, user);

        List<string> postCats = new List<string>();
        var comments = await _commentService.GetAllByPost(post.Id);

        foreach (var cat in Enum.GetValues(typeof(PostCategory)))
        {
            postCats.Add(cat.ToString());
        }

        return new PostDetailsViewModel
        {
            PostCategories = postCats,
            Post = post,
            Comment = null,
            Comments = comments,
            AccountUser = user,
            CommentCount = 0,
        };
    }

    public async Task<PostDetailsViewModel> GetPostDetailsViewModel(string id, ClaimsPrincipal claimsPrincipal)
    {
        Post post = await _postService.Get(id);
        List<string> postCats = new List<string>();
        var posts = await _postService.GetPublishedOnly("");
        var comments = await _commentService.GetAllByPost(id);
        var replies = await _commentReplyService.GetAllByPost(id);
        var user = await _userManager.GetUserAsync(claimsPrincipal);

        int commentCount = comments.Count() + replies.Count();

        foreach (var cat in Enum.GetValues(typeof(PostCategory)))
        {
            postCats.Add(cat.ToString());
        }

        return new PostDetailsViewModel
        {
            AllPosts = posts,
            PostCategories = postCats,
            Post = post,
            Comments = comments,
            CommentReplies = replies,
            AccountUser = user,
            CommentCount = commentCount
        };
    }

    public async Task<CreatePostViewModel> GetCreatePostViewModel(ClaimsPrincipal claimsPrincipal)
    {
        CreatePostViewModel createViewModel = new CreatePostViewModel();

        var user = await _userManager.GetUserAsync(claimsPrincipal);

        User loggedInUser = await _userService.Get(user.Id);

        createViewModel.AccountUser = loggedInUser;

        return createViewModel;
    }

    public async Task<Post> CreatePost(CreatePostViewModel createViewModel, ClaimsPrincipal claimsPrincipal)
    {
        Post post = createViewModel.Post;

        var user = await _userManager.GetUserAsync(claimsPrincipal);

        post.CreatedBy = user;
        post.CreatedOn = DateTime.Now;
        post.UpdatedOn = DateTime.Now;

        post = await _postService.Create(post);

        // stores image file name 
        string webRootPath = webHostEnvironment.WebRootPath;
        string pathToImage = $@"{webRootPath}\UserFiles\Posts\{post.Id}\HeaderImage.jpg";

        EnsureFolder(pathToImage);

        IFormFile headerImg = createViewModel.HeaderImage;

        using (var fileStream = new FileStream(pathToImage, FileMode.Create))
        {
            await headerImg.CopyToAsync(fileStream);
        }

        return post;
    }

    private IFormFile GetDefaultImage()
    {
        string webRootPath = webHostEnvironment.WebRootPath;
        string pathToImage = $@"{webRootPath}\images\default-image.jpg";
        using (var stream = File.OpenRead(pathToImage))
        {
            return new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));
        }
    }

    private void EnsureFolder(string path)
    {
        string directoryName = Path.GetDirectoryName(path);
        if (directoryName.Length > 0)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
    }

    public async Task<Comment> CreateComment(PostDetailsViewModel postDetailsViewModel, ClaimsPrincipal claimsPrincipal)
    {
        Comment comment = postDetailsViewModel.Comment;

        var user = await _userManager.GetUserAsync(claimsPrincipal);

        comment.CommentedPost = postDetailsViewModel.Post;
        comment.CreatedBy = user;
        comment.CreatedOn = DateTime.Now;
        comment.UpdatedOn = DateTime.Now;

        comment = await _commentService.Create(comment);

        return comment;
    }

    public async Task<CommentReply> CreateReply(PostDetailsViewModel postDetailsViewModel, ClaimsPrincipal claimsPrincipal)
    {
        CommentReply reply = postDetailsViewModel.CommentReply;

        var user = await _userManager.GetUserAsync(claimsPrincipal);
        var post = await _postService.Get(postDetailsViewModel.Post.Id);
        var comment = await _commentService.Get(postDetailsViewModel.Comment.Id);

        reply.RepliedPost = post;
        reply.RepliedComment = comment;
        reply.CreatedBy = user;
        reply.CreatedOn = DateTime.Now;
        reply.UpdatedOn = DateTime.Now;

        reply = await _commentReplyService.Create(reply);

        await _commentService.Update(comment.Id, comment);

        return reply;
    }

    public async Task<EditPostViewModel> GetEditPostViewModel(string postId, ClaimsPrincipal claimsPrincipal)
    {
        var post = await _postService.Get(postId);

        var user = await _userManager.GetUserAsync(claimsPrincipal);

        User loggedInUser = await _userService.Get(user.Id);

        return new EditPostViewModel
        {
            Post = post,
            AccountUser = loggedInUser
        };
    }

    public async Task<EditPostViewModel> GetEditPostViewModelViaComment(string commentId)
    {
        var comment = await _commentService.Get(commentId);
        var post = await _postService.Get(comment.CommentedPost.Id);

        return new EditPostViewModel
        {
            Post = post
        };
    }

    public async Task<EditPostViewModel> GetEditPostViewModelViaReply(string replyId)
    {
        var reply = await _commentReplyService.Get(replyId);
        var comment = await _commentService.Get(reply.RepliedComment.Id);
        var post = await _postService.Get(comment.CommentedPost.Id);

        return new EditPostViewModel
        {
            Post = post
        };
    }

    public async Task<ActionResult<EditPostViewModel>> EditPost(EditPostViewModel editPostViewModel, ClaimsPrincipal claimsPrincipal)
    {
        var user = await _userManager.GetUserAsync(claimsPrincipal);
        if (user is null)
            return new NotFoundResult();

        var post = await _postService.Get(editPostViewModel.Post.Id);
        if (post is null)
            return new NotFoundResult();

        post.Title = editPostViewModel.Post.Title;
        post.Category = editPostViewModel.Post.Category;
        post.Description = editPostViewModel.Post.Description;
        post.Content = editPostViewModel.Post.Content;
        post.IsPublished = editPostViewModel.Post.IsPublished;
        post.UpdatedOn = DateTime.Now;

        if (editPostViewModel.HeaderImage != null)
        {
            string webRootPath = webHostEnvironment.WebRootPath;
            string pathToImage = $@"{webRootPath}\UserFiles\Posts\{post.Id}\HeaderImage.jpg";

            EnsureFolder(pathToImage);

            using (var fileStream = new FileStream(pathToImage, FileMode.Create))
            {
                await editPostViewModel.HeaderImage.CopyToAsync(fileStream);
            }
        }

        return new EditPostViewModel
        {
            Post = await _postService.Update(editPostViewModel.Post.Id, post)
        };
    }

    public async Task<ActionResult<PostDetailsViewModel>> EditComment(string commentId, PostDetailsViewModel postDetailsViewModel, ClaimsPrincipal claimsPrincipal)
    {
        var user = await _userManager.GetUserAsync(claimsPrincipal);
        if (user is null)
            return new NotFoundResult();

        var post = await _postService.Get(postDetailsViewModel.Post.Id);
        if (post is null)
            return new NotFoundResult();

        var comment = await _commentService.Get(commentId);
        if (comment is null)
            return new NotFoundResult();

        var replies = await _commentReplyService.GetAllByPost(post.Id);

        comment.Content = postDetailsViewModel.Comment.Content;

        List<string> postCats = new List<string>();
        var comments = await _commentService.GetAllByPost(post.Id);

        int commentCount = comments.Count() + replies.Count();

        foreach (var cat in Enum.GetValues(typeof(PostCategory)))
        {
            postCats.Add(cat.ToString());
        }

        return new PostDetailsViewModel
        {
            PostCategories = postCats,
            Post = post,
            Comment = await _commentService.Update(commentId, comment),
            Comments = comments,
            AccountUser = user,
            CommentCount = commentCount
        };
    }

    public async Task<ActionResult<PostDetailsViewModel>> EditReply(string replyId, PostDetailsViewModel postDetailsViewModel, ClaimsPrincipal claimsPrincipal)
    {
        var user = await _userManager.GetUserAsync(claimsPrincipal);
        if (user is null)
            return new NotFoundResult();

        var post = await _postService.Get(postDetailsViewModel.Post.Id);
        if (post is null)
            return new NotFoundResult();

        var comment = await _commentService.Get(postDetailsViewModel.Comment.Id);
        if (comment is null)
            return new NotFoundResult();

        var replies = await _commentReplyService.GetAllByPost(post.Id);

        var reply = await _commentReplyService.Get(replyId);
        if (reply is null)
            return new NotFoundResult();

        reply.Content = postDetailsViewModel.CommentReply.Content;

        List<string> postCats = new List<string>();
        var comments = await _commentService.GetAllByPost(post.Id);

        int commentCount = comments.Count() + replies.Count();

        foreach (var cat in Enum.GetValues(typeof(PostCategory)))
        {
            postCats.Add(cat.ToString());
        }

        return new PostDetailsViewModel
        {
            PostCategories = postCats,
            Post = post,
            Comment = comment,
            Comments = comments,
            CommentReply = await _commentReplyService.Update(replyId, reply),
            AccountUser = user,
            CommentCount = commentCount
        };
    }

    public async Task<ActionResult<PostDetailsViewModel>> LikeComment(string commentId, PostDetailsViewModel postDetailsViewModel, ClaimsPrincipal claimsPrincipal)
    {
        var user = await _userManager.GetUserAsync(claimsPrincipal);
        if (user is null)
            return new NotFoundResult();

        var post = await _postService.Get(postDetailsViewModel.Post.Id);
        if (post is null)
            return new NotFoundResult();

        var comment = await _commentService.Get(commentId);
        if (comment is null)
            return new NotFoundResult();

        var replies = await _commentReplyService.GetAllByPost(post.Id);

        var userAlreadyLiked = comment.CommentLikedByUsers.Where(u => u.Id == user.Id).FirstOrDefault();

        if (userAlreadyLiked is null)
        {
            comment.CommentLikedByUsers.Add(user);
        }
        else
        {
            comment.CommentLikedByUsers.Remove(userAlreadyLiked);
        }

        List<string> postCats = new List<string>();
        var comments = await _commentService.GetAllByPost(post.Id);

        int commentCount = comments.Count() + replies.Count();

        foreach (var cat in Enum.GetValues(typeof(PostCategory)))
        {
            postCats.Add(cat.ToString());
        }

        return new PostDetailsViewModel
        {
            PostCategories = postCats,
            Post = post,
            Comment = await _commentService.Update(commentId, comment),
            Comments = comments,
            AccountUser = user,
            CommentCount = commentCount
        };
    }

    public async Task<ActionResult<Post>> DeletePost(string postId, ClaimsPrincipal claimsPrincipal)
    {
        var post = await _postService.Get(postId);
        if (post is null)
            return new NotFoundResult();

        var user = await _userManager.GetUserAsync(claimsPrincipal);
        if (user == null)
            return new NotFoundResult();

        // Validate User
        if (post.CreatedBy.Id != user.Id)
            return new NotFoundResult();

        _commentService.RemoveAllByPost(postId);
        _commentReplyService.RemoveAllByPost(postId);

        string webRootPath = webHostEnvironment.WebRootPath;
        string pathToImage = $@"{webRootPath}\UserFiles\Posts\{post.Id}";

        string[] files = Directory.GetFiles(pathToImage, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            File.Delete(file);
        }
        //then delete folder
        Directory.Delete(pathToImage);

        _postService.Remove(post);
        return post;
    }

    public async void DeleteComment(string commentId, ClaimsPrincipal claimsPrincipal)
    {

        var user = await _userManager.GetUserAsync(claimsPrincipal);
        if (user == null)
            throw new Exception("User could not be found");

        var comment = await _commentService.Get(commentId);
        if (comment == null)
            throw new Exception("comment could not be found");

        // Validate User
        if (comment.CreatedBy.Id != user.Id) 
            throw new Exception("Cannot delete because comment creator and user creater are not the same");


        // remove all replies for the comment
        _commentReplyService.RemoveAllByComment(commentId);

        // remove comment
        _commentService.Remove(commentId);
    }

    public async void DeleteReply(string replyId, ClaimsPrincipal claimsPrincipal)
    {

        var user = await _userManager.GetUserAsync(claimsPrincipal);
        if (user == null)
            throw new Exception("User could not be found");

        var reply = await _commentReplyService.Get(replyId);
        if (reply == null)
            throw new Exception("Reply could not be found");

        // Validate User
        if (reply.CreatedBy.Id != user.Id)
            throw new Exception("Cannot delete because reply creator and user creater are not the same");

        _commentReplyService.Remove(replyId);
    }
}