﻿// Handle all button interactions and search functionality
$(document).ready(function() {
    // Initialize search functionality
    const searchForm = $('.search-bar form');
    const searchInput = $('.search-bar input');
    const searchResults = $('.search-results');
    
    // Handle search form submission
    searchForm.on('submit', function(e) {
        e.preventDefault();
        const query = searchInput.val().trim();
        
        if (query.length > 0) {
            performSearch(query);
        }
    });

    // Perform search with debounce
    let searchTimeout;
    searchInput.on('input', function() {
        clearTimeout(searchTimeout);
        const query = $(this).val().trim();
        
        if (query.length > 2) {
            searchTimeout = setTimeout(() => {
                performSearch(query);
            }, 300);
        } else {
            searchResults.hide().empty();
        }
    });

    // Function to perform search
    function performSearch(query) {
        $.ajax({
            url: '/Product/Search',
            method: 'GET',
            data: { query: query },
            beforeSend: function() {
                searchResults.html('<div class="text-center py-3"><div class="spinner-border text-primary" role="status"></div></div>').show();
            },
            success: function(response) {
                if (response.success && response.results.length > 0) {
                    const resultsHtml = response.results.map(product => `
                        <div class="search-result-item">
                            <a href="/Product/Details/${product.id}" class="d-flex align-items-center p-3 text-decoration-none text-dark">
                                <img src="${product.mainImageUrl}" alt="${product.name}" class="me-3" style="width: 50px; height: 50px; object-fit: cover;">
                                <div>
                                    <div class="fw-bold">${product.name}</div>
                                    <small class="text-muted">${product.brand}</small>
                                    <div class="text-primary">$${product.price}</div>
                                </div>
                            </a>
                        </div>
                    `).join('');
                    searchResults.html(resultsHtml).show();
                } else {
                    searchResults.html('<div class="text-center py-3 text-muted">No products found</div>').show();
                }
            },
            error: function() {
                searchResults.html('<div class="text-center py-3 text-danger">Error loading search results</div>').show();
            }
        });
    }

    // Close search results when clicking outside
    $(document).on('click', function(e) {
        if (!$(e.target).closest('.search-bar').length) {
            searchResults.hide();
        }
    });

    // Add search results container to DOM
   I'll fix the syntax errors in the site.js file. Here's the correction:

<diff>
﻿// Handle all button interactions and search functionality
$(document).ready(function() {
    // Initialize search functionality
    const searchForm = $('.search-bar form');
    const searchInput = $('.search-bar input');
    const searchResults = $('.search-results');
    
    // Handle search form submission
    searchForm.on('submit', function(e) {
        e.preventDefault();
        const query = searchInput.val().trim();
        
        if (query.length > 0) {
            performSearch(query);
        }
    });

    // Perform search with debounce
    let searchTimeout;
    searchInput.on('input', function() {
        clearTimeout(searchTimeout);
        const query = $(this).val().trim();
        
        if (query.length > 2) {
            searchTimeout = setTimeout(() => {
                performSearch(query);
            }, 300);
        } else {
            searchResults.hide().empty();
        }
    });

    // Function to perform search
    function performSearch(query) {
        $.ajax({
            url: '/Product/Search',
            method: 'GET',
            data: { query: query },
            beforeSend: function() {
                searchResults.html('<div class="text-center py-3"><div class="spinner-border text-primary" role="status"></div></div>').show();
            },
            success: function(response) {
                if (response.success && response.results.length > 0) {
                    const resultsHtml = response.results.map(product => `
                        <div class="search-result-item">
                            <a href="/Product/Details/${product.id}" class="d-flex align-items-center p-3 text-decoration-none text-dark">
                                <img src="${product.mainImageUrl}" alt="${product.name}" class="me-3" style="width: 50px; height: 50px; object-fit: cover;">
                                <div>
                                    <div class="fw-bold">${product.name}</div>
                                    <small class="text-muted">${product.brand}</small>
                                    <div class="text-primary">$${product.price}</div>
                                </div>
                            </a>
                        </div>
                    `).join('');
                    searchResults.html(resultsHtml).show();
                } else {
                    searchResults.html('<div class="text-center py-3 text-muted">No products found</div>').show();
                }
            },
            error: function() {
                searchResults.html('<div class="text-center py-3 text-danger">Error loading search results</div>').show();
            }
        });
    }

    // Close search results when clicking outside
    $(document).on('click', function(e) {
        if (!$(e.target).closest('.search-bar').length) {
            searchResults.hide();
        }
    });

    // Add search results container to DOM
    searchForm.after('<div class="search-results position-absolute bg-white shadow rounded" style="width: 100%; max-width: 600px; z-index: 1000; display: none;"></div>');


    // Handle Buy Now form submission
    $(document).on('submit', 'form[action*="BuyNow"]', function(e) {
        e.preventDefault();
        const form = $(this);
        const button = form.find('button[type="submit"]');
        const originalText = button.html();
        
        button.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Processing...');
        
        $.ajax({
            url: form.attr('action'),
            method: form.attr('method'),
            data: form.serialize(),
            success: function(response) {
                if (response.success) {
                    window.location.href = response.redirectUrl;
                } else {
                    showToast('Error: ' + response.message, 'error');
                }
            },
            error: function() {
                showToast('An error occurred. Please try again.', 'error');
            },
            complete: function() {
                button.prop('disabled', false).html(originalText);
            }
        });
    });

    // Handle Add to Cart form submission
    $(document).on('submit', 'form[action*="AddToCart"]', function(e) {
        e.preventDefault();
        const form = $(this);
        const button = form.find('button[type="submit"]');
        const originalText = button.html();
        
        button.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Adding...');
        
        $.ajax({
            url: form.attr('action'),
            method: form.attr('method'),
            data: form.serialize(),
            success: function(response) {
                if (response.success) {
                    showToast('Item added to cart!', 'success');
                    updateCartCount(response.cartCount);
                } else {
                    showToast('Error: ' + response.message, 'error');
                }
            },
            error: function() {
                showToast('An error occurred. Please try again.', 'error');
            },
            complete: function() {
                button.prop('disabled', false).html(originalText);
            }
        });
    });

    // Handle Shop Now and Explore More buttons
    $(document).on('click', '.btn-primary, .btn-outline-light', function(e) {
        const button = $(this);
        const originalText = button.html();
        
        if (button.attr('disabled')) {
            return;
        }

        button.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...');
        
        setTimeout(() => {
            button.prop('disabled', false).html(originalText);
        }, 1000);
    });

    // Show toast notifications
    function showToast(message, type) {
        const toast = $(`
            <div class="toast ${type}">
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `);
        $('.toast-container').append(toast);
        toast.toast({ delay: 3000 });
        toast.toast('show');
        toast.on('hidden.bs.toast', function() {
            $(this).remove();
        });
    }

    // Update cart count in navbar
    function updateCartCount(count) {
        $('#cartCount').text(count);
    }

    // Handle category explore buttons
    $(document).on('click', '.category-overlay .btn', function(e) {
        const button = $(this);
        const originalText = button.html();
        
        button.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...');
        
        setTimeout(() => {
            button.prop('disabled', false).html(originalText);
        }, 1000);
    });
});
