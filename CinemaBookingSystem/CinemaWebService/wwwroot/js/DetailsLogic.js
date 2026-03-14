let page = 0;

function slideInfo(dir) {
    const inner = document.querySelector(".movie-info-box-inner");
    const pages = inner.children.length;

    page += dir;
    // set lower bound to 0
    if (page < 0) page = 0;
    // upper bound is max pages - 1
    if (page >= pages) page = pages - 1;
    inner.style.transform = `translateX(-${page * 100}%)`;
}