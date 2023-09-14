﻿// search bar and buttons refs
const searchBarInput = document.querySelector('#searchBarInput');
const searchBarBtn = document.querySelector('#searchBarBtn');

// post menu refs
let dropdownContent = document.querySelectorAll('.dropdown-content');
let menuIcons = document.querySelectorAll('.menu-icon');

// post and category containers refs
const postsDisplayContainer = document.querySelector('#blogsDisplayContainer');
const postCategoryListContainer = document.querySelector('#blogCategoryListContainer');
const paginationNavContainer = document.querySelector('#paginationNavContainer');
const categoryBadgeContainer = document.querySelector('#categoryBadgeContainer');

// data 
let postData = document.querySelector('div.blogData');
let postsData = document.querySelectorAll('div.blogsData');
let postCategoryData = JSON.parse(document.querySelector('#blogCategoryData').getAttribute("value"));

// pagination values 
const maxPostsPerPage = 10;
let currentPageNumber = 1;

// search string and category
let postSearchString = "";
let postCategoryIdx = 100;

// controller paths
let postDetailsPath = '/Post/PostDetails';
let postImagePath = '../UserFiles/Posts';
let createPostPath = '/Post/CreatePost';
let editPostPath = '/Post/EditPost';
let deletePostPath = '/Post/DeletePost';


const months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];


// create category badge on detail view dashboard
const createCategoryBadge = () => {
    let post = JSON.parse(postData.getAttribute("value"));

    const badgeElement = document.createElement('a');
    badgeElement.innerHTML = `<a class="badge text-decoration-none link-light cs">${post.category}</a>`;

    categoryBadgeContainer.append(badgeElement);
}

// opens modal to delete given post
const openDeletePostModal = (id) => {
    console.log('open delete Modal');
    alert(`Are you sure you want to delete post with ${id}`);
}


// sets postCategoryIdx
const setPostCategory = (selectedCategory) => {
    // toggle post category on/off upon clicking same category
    if (selectedCategory == getPostCategoryName(postCategoryIdx)) {
        postCategoryIdx = 100;
    } else {
        postCategoryIdx = getPostCategoryIdx(selectedCategory);
    }

    setPostSearchString("");
    searchBarInput.value = "";
    setPostsToDisplay();
    displayPosts();
    console.log('clicked ' + selectedCategory);
}

// lists all post cateogries on side widget
const setUpPostCategoryList = () => {
    postCategoryData.forEach(cat => {
        var catName = getPostCategoryName(cat);

        const listItemElement = document.createElement('li');
        listItemElement.innerHTML = `<a href="#" onclick="return false;">${catName}</a>`;
        listItemElement.classList = 'list-select';
        listItemElement.addEventListener('click', () => setPostCategory(catName));

        postCategoryListContainer.append(listItemElement);
    });
}

// sets postSearchString 
const setPostSearchString = (searchString) => {
    postSearchString = searchString;
    currentPageNumber = 1;
}

// sets postsToShow 
const setPostsToDisplay = () => {
    blogsToShow = [];

    let postCategory = postCategoryIdx;

    postsData.forEach(b => {
        let post = JSON.parse(b.getAttribute("value"));

        if (post.title.toString().toLowerCase().includes(postSearchString) ||
            post.description.toString().toLowerCase().includes(postSearchString) ||
            post.content.toString().toLowerCase().includes(postSearchString) ||
            postSearchString.toString() === "") {
            if (post.category == postCategory || postCategory === 100) {
                blogsToShow.push(post);
            }
        }
    });

    sortPosts();

    updatePaginationVariables(blogsToShow.length);
}

// sort posts by updated date
const sortPosts = () => {
    blogsToShow = blogsToShow.sort(
        (b1, b2) => (b1.updatedOn < b2.updatedOn) ? 1 : (b1.updatedOn > b2.updatedOn) ? -1 : 0);
}

const updatePaginationVariables = (postCount) => {
    totalPageCount = Math.ceil(postCount / maxPostsPerPage);
}

const setPageNumber = (num) => {
    if (num > totalPageCount || num < 1) {
        if (num <= 1) {
            num = 1;
            currentPageNumber = 1;
        }
        else if (num >= totalPageCount) {
            num = totalPageCount;
            currentPageNumber = totalPageCount;
        }
    }

    currentPageNumber = num;
    setPostsToDisplay();
    displayPosts();
}

// displays all posts 
const displayPosts = () => {
    createPagination();
    postsDisplayContainer.innerHTML = '';

    let searchHeading = document.createElement('h3');

    if (blogsToShow.length <= 0) {
        // no results
        const divElement = document.createElement('div');
        divElement.innerHTML =
            `<div class="col-lg-6">
                <p>Sorry... There are no related results for the above query :( </p>
            </div>`;

        if (postSearchString == "") {
            divElement.innerHTML =
                `<div class="col-lg-6">
                    <p>Find any interesting blog posts? Favorite them to view them on this dashboard!</p>
                </div>`;
        } 
        postsDisplayContainer.append(searchHeading);

        postsDisplayContainer.append(divElement);
        return;
    }

    let curPostIdx = (currentPageNumber - 1) * maxPostsPerPage;
    let postIdxOnCurrentPage = currentPageNumber * maxPostsPerPage;

    for (var i = curPostIdx; i < postIdxOnCurrentPage; i++) {

        if (blogsToShow[i] == null) {

        } else {
            var updatedOnDate = new Date(blogsToShow[i].updatedOn);
            var year = updatedOnDate.getFullYear();
            var month = months[updatedOnDate.getMonth()].substring(0, 3);
            var day = updatedOnDate.getDate();

            let eyeSVG = '<svg width="50px" height="50px" viewBox="-2.4 -2.4 28.80 28.80" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M4 4L20 20" stroke="#000000" stroke-width="1.2" stroke-linecap="round"></path> <path fill-rule="evenodd" clip-rule="evenodd" d="M6.22308 5.63732C4.19212 6.89322 2.60069 8.79137 1.73175 11.0474C1.49567 11.6604 1.49567 12.3396 1.73175 12.9526C3.31889 17.0733 7.31641 20 12 20C14.422 20 16.6606 19.2173 18.4773 17.8915L17.042 16.4562C15.6033 17.4309 13.8678 18 12 18C8.17084 18 4.89784 15.6083 3.5981 12.2337C3.54022 12.0835 3.54022 11.9165 3.5981 11.7663C4.36731 9.76914 5.82766 8.11625 7.6854 7.09964L6.22308 5.63732ZM9.47955 8.89379C8.5768 9.6272 7.99997 10.7462 7.99997 12C7.99997 14.2091 9.79083 16 12 16C13.2537 16 14.3728 15.4232 15.1062 14.5204L13.6766 13.0908C13.3197 13.6382 12.7021 14 12 14C10.8954 14 9.99997 13.1046 9.99997 12C9.99997 11.2979 10.3618 10.6802 10.9091 10.3234L9.47955 8.89379ZM15.9627 12.5485L11.4515 8.03729C11.6308 8.0127 11.8139 8 12 8C14.2091 8 16 9.79086 16 12C16 12.1861 15.9873 12.3692 15.9627 12.5485ZM18.5678 15.1536C19.3538 14.3151 19.9812 13.3259 20.4018 12.2337C20.4597 12.0835 20.4597 11.9165 20.4018 11.7663C19.1021 8.39172 15.8291 6 12 6C11.2082 6 10.4402 6.10226 9.70851 6.29433L8.11855 4.70437C9.32541 4.24913 10.6335 4 12 4C16.6835 4 20.681 6.92668 22.2682 11.0474C22.5043 11.6604 22.5043 12.3396 22.2682 12.9526C21.7464 14.3074 20.964 15.5331 19.9824 16.5682L18.5678 15.1536Z" fill="#000000"></path> </g></svg>';
            if (blogsToShow[i].isPublished) {
                eyeSVG = '<svg width="50px" height="50px" viewBox="-2.4 -2.4 28.80 28.80" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M21.335 11.4069L22.2682 11.0474L21.335 11.4069ZM21.335 12.5932L20.4018 12.2337L21.335 12.5932ZM2.66492 11.4068L1.73175 11.0474L2.66492 11.4068ZM2.66492 12.5932L1.73175 12.9526L2.66492 12.5932ZM3.5981 11.7663C4.89784 8.39171 8.17084 6 12 6V4C7.31641 4 3.31889 6.92667 1.73175 11.0474L3.5981 11.7663ZM12 6C15.8291 6 19.1021 8.39172 20.4018 11.7663L22.2682 11.0474C20.681 6.92668 16.6835 4 12 4V6ZM20.4018 12.2337C19.1021 15.6083 15.8291 18 12 18V20C16.6835 20 20.681 17.0733 22.2682 12.9526L20.4018 12.2337ZM12 18C8.17084 18 4.89784 15.6083 3.5981 12.2337L1.73175 12.9526C3.31889 17.0733 7.31641 20 12 20V18ZM20.4018 11.7663C20.4597 11.9165 20.4597 12.0835 20.4018 12.2337L22.2682 12.9526C22.5043 12.3396 22.5043 11.6604 22.2682 11.0474L20.4018 11.7663ZM1.73175 11.0474C1.49567 11.6604 1.49567 12.3396 1.73175 12.9526L3.5981 12.2337C3.54022 12.0835 3.54022 11.9165 3.5981 11.7663L1.73175 11.0474Z" fill="#000000"></path> <circle cx="12" cy="12" r="3" stroke="#000000" stroke-width="1.2" stroke-linecap="round" stroke-linejoin="round"></circle> </g></svg>';
            }

            const divElement = document.createElement('div');
            divElement.classList = "col-lg-6";
            divElement.innerHTML =
                `<!-- Posts -->
                <div class="card mb-4">
                    <a href="${postDetailsPath}/${blogsToShow[i].id}">
                        <div class="banner-tag ${getPostCategoryClass(blogsToShow[i].category)}">
                            <div>${getPostCategoryName(blogsToShow[i].category)}</div >
                        </div>
                        <img class="card-img-top" src="${postImagePath}/${blogsToShow[i].id}/HeaderImage.jpg" alt="${blogsToShow[i].title}" />
                    </a>
                    <div class="card-body">
                        <h2 class="card-title h4"><a href="${postDetailsPath}/${blogsToShow[i].id}">${blogsToShow[i].title}</a></h2>
                        <div class="small text-muted">Last Updated on ${month} ${day}, ${year} by ${blogsToShow[i].createdBy.userName}</div>
                        <p class="card-text text-truncate">${blogsToShow[i].description}</p>
                    </div>
                </div>`;

            postsDisplayContainer.append(divElement);
        }
    }

    dropdownContent = document.querySelectorAll('.dropdown-content');
    menuIcons = document.querySelectorAll('.menu-icon');
    // adds on click listener to all comment menus
    menuIcons.forEach(menu => {
        menu.addEventListener('click', () => {
            openCommentDropDownMenu(menu);
            console.log('add event listener on menu icons');
        });
    });
}

// handles pagination and creates/designs the pagination nav accordingly
const createPagination = () => {
    paginationNavContainer.innerHTML = '';
    // do pagination only if greater than maxPostsPerPage posts
    if (blogsToShow.length > maxPostsPerPage) {

        console.log('posts length: ' + blogsToShow.length);
        console.log('max posts: ' + maxPostsPerPage);
        console.log('total page count: ' + totalPageCount);

        const hrElement = document.createElement('hr');
        hrElement.classList = "my-0";

        const ulElement = document.createElement('ul');
        ulElement.classList = "pagination justify-content-center my-4";

        let prevBtnElement = document.createElement('li');
        prevBtnElement.classList = "page-item";
        prevBtnElement.addEventListener('click', () => setPageNumber(--currentPageNumber));

        if (currentPageNumber == 1) {
            prevBtnElement.innerHTML = `<a class="page-link disabled">Newer</a>`;
        } else {
            prevBtnElement.innerHTML = `<a class="page-link">Newer</a>`;
        }
        ulElement.append(prevBtnElement);

        for (let i = 0; i < totalPageCount; i++) {
            let liElement = document.createElement('li');
            liElement.classList = "page-item";
            liElement.addEventListener('click', () => setPageNumber(i + 1));
            if (currentPageNumber == (i + 1)) {
                liElement.innerHTML = `<a class="page-link font-weight-bold">${i + 1}</a>`;
            } else {
                liElement.innerHTML = `<a class="page-link">${i + 1}</a>`;
            }

            ulElement.append(liElement);
        }

        let nextBtnElement = document.createElement('li');
        nextBtnElement.classList = "page-item";
        nextBtnElement.addEventListener('click', () => setPageNumber(++currentPageNumber));

        if (currentPageNumber == totalPageCount) {
            nextBtnElement.innerHTML = `<a class="page-link disabled">Older</a>`;
        } else {
            nextBtnElement.innerHTML = `<a class="page-link">Older</a>`;
        }
        ulElement.append(nextBtnElement);

        paginationNavContainer.append(hrElement);
        paginationNavContainer.append(ulElement);
    }
}

// search bar event listener 
searchBarInput.addEventListener('input', (e) => {
    setPostSearchString(e.target.value);
    setPostsToDisplay();
    displayPosts();
});


// opens the comment drop down menu
const openCommentDropDownMenu = (element) => {
    if (element.nextElementSibling.classList.contains('block')) {
        closeAllCommentDropDownMenu();
    } else {
        closeAllCommentDropDownMenu();
        element.nextElementSibling.classList.add('block');
    }
    console.log('open drop down menu');
}

// closes all comment drop down menus
const closeAllCommentDropDownMenu = () => {
    dropdownContent.forEach(dropDown => {
        dropDown.classList.remove('block');
    });
}

// closes menus when clicking on window
window.addEventListener('click', (e) => {
    if (e.target.classList.contains('menu-icon') == false) {
        closeAllCommentDropDownMenu();
    }
})




// HELPER FUNCTIONS 
// maps enum int to its name
const getPostCategoryName = (value) => {
    switch (value) {
        case 0:
            return 'HTML';
        case 1:
            return 'CSS';
        case 2:
            return 'JavaScript';
        case 3:
            return 'C#';
        case 4:
            return 'Object-Oriented Programming';
        case 5:
            return 'Web Design';
        case 6:
            return 'Tutorials';
        case 7:
            return 'Freebies';
        case 8:
            return 'Other';
        default:
            return 'All';
    }
}

// maps enum name into ints
const getPostCategoryIdx = (value) => {
    switch (value) {
        case 'HTML':
            return 0;
        case 'CSS':
            return 1;
        case 'JavaScript':
            return 2;
        case 'C#':
            return 3;
        case 'Object-Oriented Programming':
            return 4;
        case 'Web Design':
            return 5;
        case 'Tutorials':
            return 6;
        case 'Freebies':
            return 7;
        case 'Other':
            return 8;
        default:
            return 100;
    }
}

// maps enum int to color
const getPostCategoryClass = (value) => {
    switch (value) {
        case 0:
            return 'html';
        case 1:
            return 'css';
        case 2:
            return 'js';
        case 3:
            return 'cs';
        case 4:
            return 'oop';
        case 5:
            return 'web-design';
        case 6:
            return 'tutorials';
        case 7:
            return 'freebies';
        case 8:
            return 'other';
        default:
            return 'all';
    }
}

setPostCategory("All");
setPostSearchString("")
//setUpPostCategoryList();
setPostsToDisplay();
displayPosts();


