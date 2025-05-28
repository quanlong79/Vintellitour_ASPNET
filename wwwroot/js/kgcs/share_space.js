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
    const indicators = document.querySelectorAll(`#indicators-${postId} button`);
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

document.addEventListener('DOMContentLoaded', () => {
    console.log('[ShareSpace] Initializing...');

    initFilters();
    initLikeButtons();

    // Khởi tạo current image index cho carousel mỗi post
    document.querySelectorAll('[id^="media-"]').forEach(div => {
        const postId = div.id.replace('media-', '');
        currentImageIndexByPost[postId] = 0;
    });

    console.log('[ShareSpace] Initialized successfully');
});