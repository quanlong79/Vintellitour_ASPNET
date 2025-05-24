const mockLocalStorage = {
    getItem: (key) => {
        if (key === 'userId') return '789';
        return null;
    }
};
if (typeof localStorage === 'undefined') {
    window.localStorage = mockLocalStorage;
}

document.addEventListener('DOMContentLoaded', function () {
    const state = {
        postMedia: [],
        currentImageIndex: 0,
        postId: null,
        authorId: null
    };

    //function debugLog(message, data = null) {
    //    console.log(message, data);
    //    const debugOutput = document.getElementById('debugOutput');
    //    if (debugOutput) {
    //        debugOutput.textContent += `${new Date().toLocaleTimeString()}: ${message}\n`;
    //        if (data) {
    //            debugOutput.textContent += `Data: ${JSON.stringify(data, null, 2)}\n`;
    //        }
    //        debugOutput.textContent += '---\n';
    //    }
    //}

    function formatTimeAgo(timestamp) {
        const now = new Date();
        const commentDate = new Date(timestamp);
        const diffInSeconds = Math.floor((now.getTime() - commentDate.getTime()) / 1000);

        if (diffInSeconds < 60) return `${diffInSeconds}s`;
        if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)}m`;
        if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)}h`;
        if (diffInSeconds < 604800) return `${Math.floor(diffInSeconds / 86400)}d`;
        if (diffInSeconds < 2592000) return `${Math.floor(diffInSeconds / 604800)}w`;
        return `${Math.floor(diffInSeconds / 2592000)}mo`;
    }

    function showNextImage() {
        if (state.postMedia.length <= 1) return;
        state.currentImageIndex = (state.currentImageIndex + 1) % state.postMedia.length;
        updateCurrentImage();
    }

    function showPrevImage() {
        if (state.postMedia.length <= 1) return;
        state.currentImageIndex = (state.currentImageIndex - 1 + state.postMedia.length) % state.postMedia.length;
        updateCurrentImage();
    }

    function updateCurrentImage() {
        debugLog('updateCurrentImage called', {
            currentIndex: state.currentImageIndex,
            mediaLength: state.postMedia.length
        });

        const currentImage = document.getElementById('currentImage');
        const indicators = document.querySelectorAll('#imageIndicators div');

        if (state.postMedia.length > 0 && currentImage) {
            currentImage.src = state.postMedia[state.currentImageIndex].mediaUrl;
            debugLog('Image updated', state.postMedia[state.currentImageIndex].mediaUrl);

            indicators.forEach((indicator, idx) => {
                indicator.className = idx === state.currentImageIndex
                    ? 'w-2 h-2 rounded-full cursor-pointer bg-white'
                    : 'w-2 h-2 rounded-full cursor-pointer bg-white/50';
            });
        }
    }

    function setupMediaCarousel(media) {
        state.postMedia = media || [];
        state.currentImageIndex = 0;

        const currentImage = document.getElementById('currentImage');
        const imageNavControls = document.getElementById('imageNavControls');
        const imageIndicators = document.getElementById('imageIndicators');
        imageIndicators.innerHTML = '';

        if (state.postMedia.length > 0) {
            currentImage.src = state.postMedia[0].mediaUrl;
           
            if (state.postMedia.length > 1) {
                imageNavControls.classList.remove('hidden');

                state.postMedia.forEach((_, idx) => {
                    const indicator = document.createElement('div');
                    indicator.className = idx === 0
                        ? 'w-2 h-2 rounded-full cursor-pointer bg-white'
                        : 'w-2 h-2 rounded-full cursor-pointer bg-white/50';
                    indicator.onclick = () => {
                        state.currentImageIndex = idx;
                        updateCurrentImage();
                    };
                    imageIndicators.appendChild(indicator);
                });
            } else {
                imageNavControls.classList.add('hidden');
            }
        } else {
            currentImage.src = 'https://via.placeholder.com/600x400/CCCCCC/FFFFFF?text=No+Image';
            imageNavControls.classList.add('hidden');
        }
    }

    function loadCommentsFromData(comments) {
        debugLog('loadCommentsFromData called', comments);

        const commentList = document.getElementById('commentList');
        if (!commentList) {
            debugLog('ERROR: commentList element not found!');
            return;
        }

        if (!comments || comments.length === 0) {
            commentList.innerHTML = '<p class="text-gray-500 text-center py-4">Chưa có bình luận nào. Hãy là người đầu tiên bình luận!</p>';
            debugLog('No comments to display');
            return;
        }

        debugLog(`Loading ${comments.length} comments`);
        commentList.innerHTML = '';

        comments.forEach((comment, index) => {
            debugLog(`Processing comment ${index}`, comment);

            const commentEl = document.createElement('div');
            commentEl.className = 'flex items-start gap-3 mb-4 p-2 bg-gray-50 rounded-lg';

            const commentHtml = `
                        <img src="${comment.avatar || 'https://via.placeholder.com/32x32/FF6B6B/FFFFFF?text=U'}" 
                             alt="${comment.username || 'User'}"
                             class="rounded-full w-8 h-8 flex-shrink-0" />
                        <div class="flex-1">
                            <p class="text-gray-800">
                                <span class="font-bold text-orange-600">${comment.username || 'Unknown User'}</span>
                                <span class="ml-2">${comment.content || comment.text || 'No content'}</span>
                            </p>
                            <p class="text-xs text-gray-400 mt-1">${formatTimeAgo(comment.createdAt || comment.timestamp)}</p>
                        </div>
                    `;

            commentEl.innerHTML = commentHtml;
            commentList.appendChild(commentEl);
            debugLog(`Comment ${index} added to DOM`);
        });

        debugLog('All comments loaded successfully!');
    }

    function showModal(postId, authorName, authorAvatar, authorId, media, comments) {
        debugLog('showModal called', {
            postId, authorName, authorAvatar, authorId,
            mediaCount: media ? media.length : 0,
            commentsCount: comments ? comments.length : 0
        });

        const modal = document.getElementById('commentModal');
        if (!modal) {
            debugLog('ERROR: Modal element not found!');
            return;
        }

        // Set author info
        const authorNameEl = document.getElementById('authorName');
        const authorAvatarEl = document.getElementById('authorAvatar');

        if (authorNameEl) {
            authorNameEl.innerText = authorName || 'Unknown';
            debugLog('Author name set', authorName);
        }
        if (authorAvatarEl) {
            authorAvatarEl.src = authorAvatar || 'https://via.placeholder.com/32x32/FF6B6B/FFFFFF?text=U';
            debugLog('Author avatar set', authorAvatar);
        }

        // Setup media carousel
        setupMediaCarousel(media);

        // Load comments from data
        loadCommentsFromData(comments);
        // Trong hàm showModal, thêm dòng này:
        state.postId = postId;
        state.authorId = authorId;
        // Show modal
        modal.classList.remove('hidden');
        modal.classList.add('flex');
        debugLog('Modal shown');

        // Focus on comment input
        setTimeout(() => {
            const commentInput = document.getElementById('newCommentContent');
            if (commentInput) {
                commentInput.focus();
                debugLog('Comment input focused');
            }
        }, 100);
    }

    function hideModal() {
        debugLog('hideModal called');

        const modal = document.getElementById('commentModal');
        if (modal) {
            modal.classList.add('hidden');
            modal.classList.remove('flex');
        }

        state.postMedia = [];
        state.currentImageIndex = 0;
        state.postId = null;

        const commentInput = document.getElementById('newCommentContent');
        const commentList = document.getElementById('commentList');

        if (commentInput) commentInput.value = '';
        if (commentList) commentList.innerHTML = '';

        debugLog('Modal hidden and reset');
    }

    async function postComment() {
        debugLog('postComment called');

        const commentInput = document.getElementById('newCommentContent');
        const postBtn = document.getElementById('btnPostComment');
        const content = commentInput.value.trim();

        if (!content) {
            debugLog('No content to post');
            return;
        }

        if (state.isPosting) return;

        state.isPosting = true;
        postBtn.textContent = 'Đang đăng...';
        postBtn.disabled = true;

        try {
            const response = await fetch('/post/comment', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    postId: state.postId,
                    content: content
                })
            });

            const result = await response.json();

            if (result.success) {
                // Thêm comment mới vào UI
                addCommentToUI(result.comment);
                commentInput.value = '';
                debugLog('Comment posted successfully', result.comment);
            } else {
                alert(result.message || 'Có lỗi xảy ra khi đăng bình luận');
            }
        } catch (error) {
            debugLog('Error posting comment', error);
            alert('Có lỗi xảy ra khi đăng bình luận');
        } finally {
            state.isPosting = false;
            postBtn.textContent = 'Đăng';
            postBtn.disabled = false;
        }
    }
    function addCommentToUI(comment) {
        const commentList = document.getElementById('commentList');

        const commentEl = document.createElement('div');
        commentEl.className = 'flex items-start gap-3 mb-4 p-2 bg-gray-50 rounded-lg';

        commentEl.innerHTML = `
        <img src="${comment.avatar}" 
             alt="${comment.username}"
             class="rounded-full w-8 h-8 flex-shrink-0" />
        <div class="flex-1">
            <p class="text-gray-800">
                <span class="font-bold text-orange-600">${comment.username}</span>
                <span class="ml-2">${comment.content}</span>
            </p>
            <p class="text-xs text-gray-400 mt-1">${formatTimeAgo(comment.createdAt)}</p>
        </div>
    `;

        commentList.appendChild(commentEl);
    }
    // Event listeners
    const prevBtn = document.getElementById('prevImageBtn');
    const nextBtn = document.getElementById('nextImageBtn');
    const postBtn = document.getElementById('btnPostComment');
    const commentInput = document.getElementById('newCommentContent');

    if (prevBtn) prevBtn.addEventListener('click', showPrevImage);
    if (nextBtn) nextBtn.addEventListener('click', showNextImage);
    if (postBtn) postBtn.addEventListener('click', postComment);

    if (commentInput) {
        commentInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                postComment();
            }
        });
    }

    // Close modal events
    document.querySelectorAll('.modal-close, .modal-overlay').forEach(el => {
        el.addEventListener('click', hideModal);
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            hideModal();
        }
    });

    // Test button event
    const testBtn = document.getElementById('testModalBtn');
    if (testBtn) {
        testBtn.addEventListener('click', function () {
            debugLog('Test button clicked');

            const postId = this.getAttribute('data-postid');
            const authorName = this.getAttribute('data-author-name') || '';
            const authorAvatar = this.getAttribute('data-author-avatar') || '/img/VN.jpg';
            const authorId = this.getAttribute('data-author-id') || '';

            let media = [];
            try {
                const mediaAttr = this.getAttribute('data-media');
                if (mediaAttr) {
                    media = JSON.parse(mediaAttr);
                }
            } catch (error) {
                debugLog('Error parsing media data', error);
            }

            let comments = [];
            try {
                const commentsAttr = this.getAttribute('data-comments');
                if (commentsAttr && commentsAttr !== 'null') {
                    comments = JSON.parse(commentsAttr);
                }
            } catch (error) {
                debugLog('Error parsing comments data', error);
            }

            showModal(postId, authorName, authorAvatar, authorId, media, comments);
        });
    }

    // Comment button handlers
    document.querySelectorAll('.comment-button').forEach(button => {
        button.addEventListener('click', function () {
            debugLog('Comment button clicked');

            const postId = this.getAttribute('data-postid');
            const authorName = this.getAttribute('data-author-name') || '';
            const authorAvatar = this.getAttribute('data-author-avatar') || '/img/VN.jpg';
            const authorId = this.getAttribute('data-author-id') || '';

            let media = [];
            try {
                const mediaAttr = this.getAttribute('data-media');
                if (mediaAttr) {
                    media = JSON.parse(mediaAttr);
                }
            } catch (error) {
                debugLog('Error parsing media data', error);
            }

            let comments = [];
            try {
                const commentsAttr = this.getAttribute('data-comments');
                if (commentsAttr && commentsAttr !== 'null') {
                    comments = JSON.parse(commentsAttr);
                }
            } catch (error) {
                debugLog('Error parsing comments data', error);
            }


            showModal(postId, authorName, authorAvatar, authorId, media, comments);
        });
    });

    debugLog('Comment Modal initialized successfully');
});