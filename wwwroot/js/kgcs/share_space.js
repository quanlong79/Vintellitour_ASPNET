// Xử lý mở rộng/thu gọn nội dung bài viết
function toggleExpand(postId) {
    const content = document.getElementById("content-" + postId);
    const btn = document.getElementById("btn-expand-" + postId);
    if (content.classList.contains("line-clamp-3")) {
        content.classList.remove("line-clamp-3");
        btn.innerText = "Thu gọn";
    } else {
        content.classList.add("line-clamp-3");
        btn.innerText = "Xem thêm";
    }
}

const currentImageIndexByPost = {};

// Hiển thị ảnh carousel cho post
function showImage(postId, index) {
    const images = document.querySelectorAll(`#media-${postId} img`);
    const indicators = document.querySelectorAll(`#indicators-${postId} button`); // <-- sửa ở đây
    console.log('Found images:', images.length);
    console.log('Found indicators:', indicators.length);
    images.forEach((img, i) => {
        img.style.display = i === index ? "block" : "none";
    });
    indicators.forEach((dot, i) => {
        dot.classList.toggle("bg-white", i === index);
        dot.classList.toggle("bg-white/50", i !== index);
    });
    currentImageIndexByPost[postId] = index;
}

function showNextImage(postId) {
    const images = document.querySelectorAll(`#media-${postId} img`);
    if (!images.length) return;
    const length = images.length;
    let currentIndex = currentImageIndexByPost[postId] ?? 0;
    showImage(postId, (currentIndex + 1) % length);
}

function showPrevImage(postId) {
    const images = document.querySelectorAll(`#media-${postId} img`);
    if (!images.length) return;
    const length = images.length;
    let currentIndex = currentImageIndexByPost[postId] ?? 0;
    showImage(postId, (currentIndex - 1 + length) % length);
}

function initFilters() {
    const toggleBtn = document.getElementById("toggleShowMyPostsBtn");
    if (toggleBtn) {
        toggleBtn.addEventListener("click", () => {
            const input = document.getElementById("showOnlyMyPostsInput");
            input.value = input.value === "true" ? "false" : "true";
            document.getElementById("filterForm").submit();
        });
    }

    const resetBtn = document.getElementById("resetShowMyPosts");
    if (resetBtn) {
        resetBtn.addEventListener("click", () => {
            document.getElementById("showOnlyMyPostsInput").value = "false";
            document.getElementById("filterForm").submit();
        });
    }

    const sortBtn = document.getElementById("toggleSortOrderBtn");
    if (sortBtn) {
        sortBtn.addEventListener("click", () => {
            const input = document.getElementById("sortOrderInput");
            input.value = input.value === "newest" ? "oldest" : "newest";
            document.getElementById("filterForm").submit();
        });
    }

    const clearProvinceBtn = document.getElementById("ClearfilterProvinces");
    if (clearProvinceBtn) {
        clearProvinceBtn.addEventListener("click", () => {
            document.getElementById("selectedProvinceId").value = "";
            document.getElementById("filterForm").submit();
        });
    }
}

function initLikeButtons() {
    document.querySelectorAll('.like-button').forEach(button => {
        button.addEventListener('click', async () => {
            const postId = button.getAttribute('data-postid');
            try {
                const response = await fetch('/post/togglelike', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
                    body: JSON.stringify({ postId })
                });
                if (!response.ok) {
                    alert('Lỗi khi cập nhật like!');
                    return;
                }
                const result = await response.json();
                if (result.success) {
                    button.setAttribute('data-liked', result.isLiked.toString());
                    const svg = button.querySelector('svg');
                    if (svg) svg.setAttribute('fill', result.isLiked ? 'red' : 'none');
                    const likesCountEl = button.querySelector('.likes-count');
                    if (likesCountEl) likesCountEl.innerText = result.likesCount;
                }
            } catch {
                alert('Lỗi khi kết nối server!');
            }
        });
    });
}

// Hàm gọi mở modal comment (được xử lý riêng trong commentModal.js)
function openCommentModal(postId, authorName, authorAvatar, authorId, media, comments) {
    if (typeof window.commentModal === 'object' && typeof window.commentModal.show === 'function') {
        window.commentModal.show(postId, authorName, authorAvatar, authorId, media,comments);
    } else {
        console.error('commentModal module chưa được load');
        return;
    }

    const commentList = document.getElementById('commentList');

    if (!commentList) {
        console.error('Element commentList không tồn tại');
        return;
    }
 
}


    
document.addEventListener('DOMContentLoaded', () => {
    initFilters();
    initLikeButtons();

    // Khởi tạo current image index cho carousel mỗi post
    document.querySelectorAll('[id^="media-"]').forEach(div => {
        const postId = div.id.replace('media-', '');
        currentImageIndexByPost[postId] = 0;
    });

    // Gắn sự kiện gọi modal comment
    document.querySelectorAll('.comment-button').forEach(button => {
        button.addEventListener('click', () => {
            const postId = button.getAttribute('data-postid');
            const authorName = button.getAttribute('data-author-name') || '';
            const authorAvatar = button.getAttribute('data-author-avatar') || '/img/VN.jpg';
            const authorId = button.getAttribute('data-author-id') || '';

            let media = [];
            try {
                const mediaAttr = button.getAttribute('data-media');
                if (mediaAttr && mediaAttr !== 'null') {
                    media = JSON.parse(mediaAttr);
                }
            } catch (e) {
                console.error('Error parsing media JSON:', e);
            }

            let comments = [];
            try {
                const commentsAttr = button.getAttribute('data-comments');
                if (commentsAttr && commentsAttr !== 'null') {
                    comments = JSON.parse(commentsAttr);
                }
            } catch (e) {
                console.error('Error parsing comments JSON:', e);
            }

            console.log('Opening modal with:', { postId, authorName, comments }); // Debug log
            openCommentModal(postId, authorName, authorAvatar, authorId, media, comments);
        });
    });
});
