// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', function () {
	initializeFlowAnimations();
	initializeDatePickers();

	document.querySelectorAll('[data-ajax-filter]').forEach(function (searchInput) {
		initializeAjaxFilter(searchInput);
	});

	document.querySelectorAll('[data-autocomplete-dropdown]').forEach(function (dropdown) {
		initializeAutocompleteDropdown(dropdown);
	});

	document.querySelectorAll('form[data-purchase-related-options-url]').forEach(function (form) {
		initializePurchaseRelatedDropdowns(form);
	});

	configureBlurValidation();
	initializeValidationSummaryAnimations();
	initializeFormSubmissionAnimations();
	initializeRequiredFieldMarkers();
});

function initializeRequiredFieldMarkers() {
	// Add a visual '*' next to labels for required fields (server- or client-declared)
	document.querySelectorAll('form').forEach(function (form) {
		['input', 'select', 'textarea'].forEach(function (selector) {
			form.querySelectorAll(selector).forEach(function (el) {
				var isRequired = el.hasAttribute('required') || el.getAttribute('data-val-required');
				if (!isRequired) return;
				var id = el.id;
				if (!id) return;
				var label = form.querySelector('label[for="' + id + '"]');
				if (!label) return;
				if (label.querySelector('.required-indicator')) return;
				var span = document.createElement('span');
				span.className = 'text-danger ms-1 required-indicator';
				span.textContent = '*';
				label.appendChild(span);
			});
		});
	});
}

function initializeDatePickers() {
	const pickerElements = document.querySelectorAll('[data-date-picker]');
	if (pickerElements.length === 0) {
		return;
	}

	import('/js/date-picker.js').then(function(module) {
		pickerElements.forEach(function(picker) {
			module.initializeDatePicker(picker);
		});
	});
}

function initializeFlowAnimations() {
	const root = document.documentElement;
	const prefersReducedMotion = shouldReduceMotion();

	requestAnimationFrame(function () {
		root.classList.add('js-ready');

		if (prefersReducedMotion) {
			root.classList.add('motion-reduce');
		} else {
			root.classList.add('motion-safe');
		}

		initializePageReveal(prefersReducedMotion);
	});
}

function initializePageReveal(prefersReducedMotion) {
	const revealTargets = document.querySelectorAll([
		'.home-dashboard > section',
		'.index-shell > *',
		'.details-shell > *',
		'.form-shell > *',
		'.delete-shell > *'
	].join(','));

	if (revealTargets.length === 0 || prefersReducedMotion) {
		return;
	}

	revealTargets.forEach(function (element, index) {
		element.classList.add('anim-reveal-item');
		setTimeout(function () {
			element.classList.add('is-visible');
		}, 40 + index * 70);
	});
}

function configureBlurValidation() {
	if (!window.jQuery || !window.jQuery.validator) {
		return;
	}

	window.jQuery.validator.setDefaults({
		ignore: ':hidden:not(.autocomplete-dropdown__value):not(.date-picker-value)',
		onfocusout: false,
		onkeyup: false,
		onclick: false,
		highlight: function (element) {
			element.classList.add('is-invalid');
			animateValidationState(element, 'invalid');
		},
		unhighlight: function (element) {
			element.classList.remove('is-invalid');
			element.classList.remove('is-valid');
		}
	});

	window.jQuery('form').each(function () {
		const form = window.jQuery(this);
		if (!form.data('validator')) {
			form.validate();
		}
	});
}

function animateValidationState(element, state) {
	if (shouldReduceMotion()) {
		return;
	}

	if (state === 'invalid') {
		element.classList.remove('anim-field-valid-settle');
		element.classList.remove('anim-field-invalid-pulse');
		void element.offsetWidth;
		element.classList.add('anim-field-invalid-pulse');
		return;
	}

	element.classList.remove('anim-field-invalid-pulse');
	element.classList.remove('anim-field-valid-settle');
	void element.offsetWidth;
	element.classList.add('anim-field-valid-settle');
}

function initializeValidationSummaryAnimations() {
	const summaries = document.querySelectorAll('[data-valmsg-summary="true"]');

	summaries.forEach(function (summary) {
		const list = summary.querySelector('ul');

		function syncState() {
			const hasErrors = list && list.children.length > 0;
			summary.classList.toggle('anim-summary-visible', Boolean(hasErrors));
		}

		syncState();

		if (!list) {
			return;
		}

		const observer = new MutationObserver(syncState);
		observer.observe(list, { childList: true, subtree: false });
	});
}

function initializeFormSubmissionAnimations() {
	document.querySelectorAll('form').forEach(function (form) {
		form.addEventListener('submit', function () {
			if (!isFormValidForSubmit(form)) {
				return;
			}

			const isDeleteFlow = isDeleteForm(form);
			const pendingText = form.dataset.submitPendingText || (isDeleteFlow ? 'Deleting...' : 'Saving...');
			setSubmitPending(form, pendingText, isDeleteFlow);
		});
	});
}

function isFormValidForSubmit(form) {
	if (window.jQuery && window.jQuery.validator) {
		const jqForm = window.jQuery(form);
		if (jqForm.data('validator') && !jqForm.valid()) {
			return false;
		}
	}

	if (typeof form.checkValidity === 'function' && !form.checkValidity()) {
		return false;
	}

	return true;
}

function isDeleteForm(form) {
	if (form.classList.contains('delete-actions')) {
		return true;
	}

	const action = (form.getAttribute('action') || '').toLowerCase();
	if (action.endsWith('/delete')) {
		return true;
	}

	return form.querySelector('.btn-danger') !== null;
}

function setSubmitPending(form, pendingText, isDeleteFlow) {
	if (form.dataset.pendingSubmit === 'true') {
		return;
	}

	form.dataset.pendingSubmit = 'true';
	form.classList.add('anim-submit-pending');

	const surface = form.closest('.form-surface, .delete-surface, .index-surface');
	if (surface) {
		surface.classList.add('anim-disabled-surface');
	}

	const submitButtons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
	submitButtons.forEach(function (button) {
		button.disabled = true;
		button.classList.add('is-pending');

		if (button.tagName === 'BUTTON') {
			if (!button.dataset.originalText) {
				button.dataset.originalText = button.textContent.trim();
			}
			button.textContent = pendingText;
		} else if (button.tagName === 'INPUT') {
			if (!button.dataset.originalText) {
				button.dataset.originalText = button.value;
			}
			button.value = pendingText;
		}
	});

	if (isDeleteFlow) {
		form.classList.add('anim-delete-pending');
	}

	emitFlowEvent('form:submit-pending', {
		deleteFlow: isDeleteFlow,
		action: form.getAttribute('action') || ''
	});
}

function emitFlowEvent(name, detail) {
	window.dispatchEvent(new CustomEvent(name, { detail: detail }));
}

function shouldReduceMotion() {
	return window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
}

function initializeAjaxFilter(searchInput) {
	const endpoint = searchInput.dataset.url;
	const tableBody = document.querySelector(searchInput.dataset.target);

	if (!endpoint || !tableBody) {
		return;
	}

	const countChip = searchInput.dataset.countTarget ? document.querySelector(searchInput.dataset.countTarget) : null;
	const singularLabel = searchInput.dataset.singular || 'item';
	const pluralLabel = searchInput.dataset.plural || `${singularLabel}s`;
	const emptyMessage = searchInput.dataset.emptyMessage || 'No matching results.';
	const columnCount = Number(searchInput.dataset.columnCount || '1');
	const resultsSurface = tableBody.closest('.index-surface, .table-responsive') || tableBody;

	let activeRequest = null;
	let requestSequence = 0;

	function updateCount(count) {
		if (countChip) {
			countChip.textContent = `${count} ${count === 1 ? singularLabel : pluralLabel}`;
		}
	}

	function renderEmptyState() {
		return `<tr class="anim-empty-enter"><td colspan="${columnCount}" class="text-center text-muted py-4">${emptyMessage}</td></tr>`;
	}

	function setLoadingState(isLoading) {
		searchInput.classList.toggle('anim-loading-inline', isLoading);
		resultsSurface.classList.toggle('anim-loading', isLoading);
	}

	function animateRowsIn() {
		if (shouldReduceMotion()) {
			return;
		}

		const rows = tableBody.querySelectorAll('tr');
		rows.forEach(function (row, index) {
			row.classList.add('anim-row-enter');
			row.style.setProperty('--row-delay', `${index * 35}ms`);
		});
	}

	function replaceTableContent(html) {
		if (shouldReduceMotion()) {
			tableBody.innerHTML = html;
			animateRowsIn();
			return Promise.resolve();
		}

		tableBody.classList.add('anim-fade-swap-out');

		return new Promise(function (resolve) {
			setTimeout(function () {
				tableBody.innerHTML = html;
				tableBody.classList.remove('anim-fade-swap-out');
				tableBody.classList.add('anim-fade-swap-in');
				animateRowsIn();

				setTimeout(function () {
					tableBody.classList.remove('anim-fade-swap-in');
					resolve();
				}, 210);
			}, 130);
		});
	}

	async function refreshResults(query) {
		requestSequence += 1;
		const currentSequence = requestSequence;

		if (activeRequest) {
			activeRequest.abort();
		}

		activeRequest = new AbortController();
		setLoadingState(true);
		emitFlowEvent('filter:loading', { endpoint: endpoint, query: query });

		try {
			const response = await fetch(`${endpoint}?query=${encodeURIComponent(query)}`, {
				signal: activeRequest.signal
			});

			if (!response.ok) {
				throw new Error(`Request failed with status ${response.status}`);
			}

			if (currentSequence !== requestSequence) {
				return;
			}

			const html = (await response.text()).trim();
			const preview = document.createElement('tbody');
			preview.innerHTML = html;
			const rowCount = preview.querySelectorAll('tr').length;
			const renderedHtml = rowCount > 0 ? html : renderEmptyState();

			await replaceTableContent(renderedHtml);
			updateCount(rowCount);

			emitFlowEvent('filter:done', {
				endpoint: endpoint,
				query: query,
				rowCount: rowCount
			});
		} finally {
			if (currentSequence === requestSequence) {
				setLoadingState(false);
			}
		}
	}

	searchInput.addEventListener('input', debounce(function (event) {
		refreshResults(event.target.value).catch(function (error) {
			if (error.name !== 'AbortError') {
				console.error('AJAX filter failed:', error);
			}
		});
	}, 300));
}

function debounce(callback, delay) {
	let timeoutId;

	return function () {
		const args = arguments;
		clearTimeout(timeoutId);
		timeoutId = setTimeout(function () {
			callback.apply(null, args);
		}, delay);
	};
}

function initializeAutocompleteDropdown(dropdown) {
	const input = dropdown.querySelector('[data-autocomplete-input]');
	const resultList = dropdown.querySelector('[data-autocomplete-results]');
	if (!input || !resultList) {
		return;
	}

	const hiddenInputId = input.dataset.autocompleteTarget;
	const hiddenInput = document.getElementById(hiddenInputId);
	const endpoint = input.dataset.autocompleteUrl;
	const emptyMessage = dropdown.dataset.emptyMessage || 'No matching results.';
	const requiredIntegerValue = hiddenInput.dataset.requiredInt === 'true';
	const emptyValue = requiredIntegerValue ? '0' : '';

	if (!hiddenInput || !endpoint) {
		return;
	}

	let activeRequest = null;
	let selectedText = input.value || '';
	let selectionCleared = false;

	function hideResults() {
		resultList.classList.add('d-none');
		resultList.classList.remove('anim-fade-swap-in');
		resultList.innerHTML = '';
	}

	function showMessage(message) {
		resultList.innerHTML = `<button type="button" class="list-group-item list-group-item-action disabled">${message}</button>`;
		resultList.classList.remove('d-none');
	}

	function selectOption(option) {
		hiddenInput.value = option.id;
		input.value = option.text;
		selectedText = option.text;
		selectionCleared = false;
		hiddenInput.dispatchEvent(new Event('change', { bubbles: true }));
		validateAutocompleteValue(input, hiddenInput);
		hideResults();
	}

	function renderResults(options) {
		if (!options || options.length === 0) {
			showMessage(emptyMessage);
			return;
		}

		resultList.innerHTML = '';
		options.forEach(function (option) {
			const button = document.createElement('button');
			button.type = 'button';
			button.className = 'list-group-item list-group-item-action';
			button.textContent = option.text;
			button.addEventListener('mousedown', function (event) {
				event.preventDefault();
				selectOption(option);
			});
			resultList.appendChild(button);
		});

		resultList.classList.remove('d-none');
		resultList.classList.add('anim-fade-swap-in');
	}

	async function fetchOptions(query) {
		if (activeRequest) {
			activeRequest.abort();
		}

		activeRequest = new AbortController();
		const response = await fetch(`${endpoint}?query=${encodeURIComponent(query)}`, {
			signal: activeRequest.signal
		});

		if (!response.ok) {
			throw new Error(`Request failed with status ${response.status}`);
		}

		return response.json();
	}

	input.addEventListener('input', debounce(function () {
		const query = input.value.trim();
		if (query !== selectedText) {
			if (hiddenInput.value !== emptyValue) {
				hiddenInput.value = emptyValue;
				selectionCleared = true;
			}
		}

		if (query.length < 1) {
			hideResults();
			dropdown.classList.remove('anim-loading-inline');
			return;
		}

		fetchOptions(query)
			.then(renderResults)
			.catch(function (error) {
				if (error.name !== 'AbortError') {
					console.error('Autocomplete failed:', error);
				}
				hideResults();
			})
			.finally(function () {
				dropdown.classList.remove('anim-loading-inline');
			});
	}, 250));

	input.addEventListener('focus', function () {
		if (input.value.trim().length > 0) {
			dropdown.classList.add('anim-loading-inline');
			fetchOptions(input.value.trim())
				.then(renderResults)
				.catch(function () {
					hideResults();
				})
				.finally(function () {
					dropdown.classList.remove('anim-loading-inline');
				});
		}
	});

	input.addEventListener('blur', function () {
		setTimeout(function () {
			validateAutocompleteValue(input, hiddenInput);

			if (hiddenInput.value === emptyValue) {
				input.value = '';
				selectedText = '';
				if (selectionCleared) {
					hiddenInput.dispatchEvent(new Event('change', { bubbles: true }));
				}
				selectionCleared = false;
			}
			hideResults();
		}, 120);
	});

	input.addEventListener('input', function () {
		dropdown.classList.add('anim-loading-inline');
	});
}

function initializePurchaseRelatedDropdowns(form) {
	const endpoint = form.dataset.purchaseRelatedOptionsUrl;
	const userInput = form.querySelector('input[name="UserProfileId"]');
	const spendingSessionSelect = form.querySelector('select[name="SpendingSessionId"]');
	const budgetPlanSelect = form.querySelector('select[name="BudgetPlanId"]');
	const wishlistItemSelect = form.querySelector('select[name="WishlistItemId"]');
	const amountInput = form.querySelector('input[name="Amount"]');

	if (!endpoint || !userInput) {
		return;
	}

	const dropdowns = [
		{ select: spendingSessionSelect, key: 'spendingSessions' },
		{ select: budgetPlanSelect, key: 'budgetPlans' },
		{ select: wishlistItemSelect, key: 'wishlistItems' }
	].filter(function (item) {
		return Boolean(item.select);
	});

	if (dropdowns.length === 0) {
		return;
	}

	let activeRequest = null;
	let wishlistPriceMap = {};

	function getPlaceholderText(select) {
		return select.options.length > 0 ? select.options[0].textContent : 'Select an option...';
	}

	function populateSelect(select, options) {
		const currentValue = select.value;
		const placeholderText = getPlaceholderText(select);
		const optionExists = options.some(function (option) {
			return String(option.id) === String(currentValue);
		});

		select.innerHTML = '';

		const placeholder = document.createElement('option');
		placeholder.value = '';
		placeholder.textContent = placeholderText;
		select.appendChild(placeholder);

		options.forEach(function (option) {
			const selectOption = document.createElement('option');
			selectOption.value = option.id;
			selectOption.textContent = option.text;
			if (typeof option.currentPrice !== 'undefined') {
				selectOption.dataset.currentPrice = String(option.currentPrice);
			}
			select.appendChild(selectOption);
		});

		select.value = optionExists ? currentValue : '';
	}

	function setAmountFromWishlistSelect() {
		if (!amountInput || !wishlistItemSelect) {
			return;
		}

		const selectedOption = wishlistItemSelect.selectedOptions && wishlistItemSelect.selectedOptions.length > 0
			? wishlistItemSelect.selectedOptions[0]
			: null;
		if (!selectedOption) {
			return;
		}

		const currentPrice = selectedOption.dataset.currentPrice || wishlistPriceMap[wishlistItemSelect.value];
		if (!currentPrice) {
			return;
		}

		amountInput.value = currentPrice;
		amountInput.dispatchEvent(new Event('input', { bubbles: true }));
		amountInput.dispatchEvent(new Event('change', { bubbles: true }));
	}

	function clearSelects() {
		dropdowns.forEach(function (item) {
			populateSelect(item.select, []);
		});
	}

	async function refreshOptions() {
		const userProfileId = userInput.value.trim();

		if (!userProfileId || userProfileId === '0') {
			clearSelects();
			return;
		}

		if (activeRequest) {
			activeRequest.abort();
		}

		activeRequest = new AbortController();
		const response = await fetch(`${endpoint}?userProfileId=${encodeURIComponent(userProfileId)}`, {
			signal: activeRequest.signal
		});

		if (!response.ok) {
			throw new Error(`Request failed with status ${response.status}`);
		}

		const data = await response.json();
		wishlistPriceMap = {};
		(data.wishlistPrices || []).forEach(function (item) {
			wishlistPriceMap[String(item.id)] = String(item.currentPrice);
		});
		dropdowns.forEach(function (item) {
			populateSelect(item.select, data[item.key] || []);
		});
		setAmountFromWishlistSelect();
	}

	if (wishlistItemSelect) {
		wishlistItemSelect.addEventListener('change', function () {
			setAmountFromWishlistSelect();
		});
	}

	userInput.addEventListener('change', function () {
		refreshOptions().catch(function (error) {
			if (error.name !== 'AbortError') {
				console.error('Purchase dropdown refresh failed:', error);
			}
		});
	});

	refreshOptions().catch(function (error) {
		if (error.name !== 'AbortError') {
			console.error('Purchase dropdown refresh failed:', error);
		}
	});
}

function validateAutocompleteValue(searchInput, hiddenInput) {
	if (!window.jQuery || !window.jQuery.validator) {
		return;
	}

	const formElement = searchInput.closest('form');
	if (!formElement) {
		return;
	}

	const form = window.jQuery(formElement);
	const validator = form.data('validator') || form.validate();
	if (validator) {
		validator.element(hiddenInput);
	}
}
