/**
 * Custom Date/DateTime Picker with Culture Support
 * Respects browser language (hr-HR, en-US, etc.)
 * Does NOT use native date input - fully custom implementation
 */

const DatePickerLocales = {
    'hr': {
        months: ['Siječanj', 'Veljača', 'Ožujak', 'Travanj', 'Svibanj', 'Lipanj', 
                 'Srpanj', 'Kolovoz', 'Rujan', 'Listopad', 'Studeni', 'Prosinac'],
        shortMonths: ['Sij', 'Vel', 'Ožu', 'Tra', 'Svi', 'Lip', 'Srp', 'Kol', 'Ruj', 'Lis', 'Stu', 'Pro'],
        days: ['Nedjelja', 'Ponedjeljak', 'Utorak', 'Srijeda', 'Četvrtak', 'Petak', 'Subota'],
        shortDays: ['Ne', 'Po', 'Ut', 'Sr', 'Če', 'Pe', 'Su'],
        format: 'dd.mm.yyyy',
        separator: '.',
        firstDay: 1 // Monday
    },
    'en': {
        months: ['January', 'February', 'March', 'April', 'May', 'June',
                 'July', 'August', 'September', 'October', 'November', 'December'],
        shortMonths: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        days: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
        shortDays: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
        format: 'mm/dd/yyyy',
        separator: '/',
        firstDay: 0 // Sunday
    }
};

function detectLocale() {
    const nav = navigator.language || navigator.userLanguage;
    const lang = nav.split('-')[0].toLowerCase();
    return DatePickerLocales[lang] || DatePickerLocales['en'];
}

function formatDate(date, locale, includeTime = false) {
    if (!date) return '';
    
    const d = String(date.getDate()).padStart(2, '0');
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const y = date.getFullYear();
    
    let formatted = locale.format
        .replace('dd', d)
        .replace('mm', m)
        .replace('yyyy', y);
    
    if (includeTime) {
        const h = String(date.getHours()).padStart(2, '0');
        const min = String(date.getMinutes()).padStart(2, '0');
        formatted += ` ${h}:${min}`;
    }
    
    return formatted;
}

function parseDate(dateString, locale) {
    if (!dateString || dateString.trim() === '') return null;
    
    const parts = dateString.trim().split(/[\s:\/\.\-]/);
    let day, month, year, hour = 0, minute = 0;
    
    if (locale.format === 'dd.mm.yyyy') {
        day = parseInt(parts[0]);
        month = parseInt(parts[1]);
        year = parseInt(parts[2]);
        if (parts[3]) hour = parseInt(parts[3]);
        if (parts[4]) minute = parseInt(parts[4]);
    } else {
        month = parseInt(parts[0]);
        day = parseInt(parts[1]);
        year = parseInt(parts[2]);
        if (parts[3]) hour = parseInt(parts[3]);
        if (parts[4]) minute = parseInt(parts[4]);
    }
    
    if (isNaN(day) || isNaN(month) || isNaN(year)) return null;
    if (month < 1 || month > 12 || day < 1 || day > 31) return null;
    
    const date = new Date(year, month - 1, day, hour, minute);
    if (date.getMonth() !== month - 1) return null;
    
    return date;
}

function toISOString(date) {
    if (!date) return '';
    
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    const h = String(date.getHours()).padStart(2, '0');
    const min = String(date.getMinutes()).padStart(2, '0');
    
    return `${y}-${m}-${d}T${h}:${min}`;
}

function renderCalendar(container, date, locale, onSelectDate) {
    const year = date.getFullYear();
    const month = date.getMonth();
    
    let html = `
        <div class="date-picker-header">
            <button type="button" class="date-picker-nav-btn date-picker-prev" data-nav="prev" aria-label="Previous month">‹</button>
            <div class="date-picker-month-year">
                <select class="date-picker-month-select" data-select="month">
    `;
    
    locale.months.forEach((m, i) => {
        html += `<option value="${i}" ${i === month ? 'selected' : ''}>${m}</option>`;
    });
    
    html += `</select>
            <select class="date-picker-year-select" data-select="year">`;
    
    for (let y = year - 10; y <= year + 10; y++) {
        html += `<option value="${y}" ${y === year ? 'selected' : ''}>${y}</option>`;
    }
    
    html += `</select>
            </div>
            <button type="button" class="date-picker-nav-btn date-picker-next" data-nav="next" aria-label="Next month">›</button>
        </div>
        <div class="date-picker-weekdays">
    `;
    
    const firstDay = locale.firstDay;
    const daysArray = firstDay === 0 ? locale.shortDays : [...locale.shortDays.slice(1), locale.shortDays[0]];
    
    daysArray.forEach(d => {
        html += `<div class="date-picker-weekday">${d}</div>`;
    });
    
    html += `</div><div class="date-picker-days">`;
    
    const firstDate = new Date(year, month, 1);
    let dayOfWeek = firstDate.getDay();
    if (firstDay === 1) {
        dayOfWeek = (dayOfWeek === 0) ? 6 : dayOfWeek - 1;
    }
    
    for (let i = 0; i < dayOfWeek; i++) {
        html += `<div class="date-picker-day date-picker-day-empty"></div>`;
    }
    
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const today = new Date();
    
    for (let d = 1; d <= daysInMonth; d++) {
        const cellDate = new Date(year, month, d);
        const isToday = cellDate.toDateString() === today.toDateString();
        const classes = `date-picker-day ${isToday ? 'date-picker-today' : ''}`;
        
        html += `<button type="button" class="${classes}" data-date="${d}" data-month="${month}" data-year="${year}">${d}</button>`;
    }
    
    html += `</div>`;
    
    container.innerHTML = html;
    
    container.addEventListener('click', (e) => {
        if (e.target.classList.contains('date-picker-day') && !e.target.classList.contains('date-picker-day-empty')) {
            const d = parseInt(e.target.dataset.date);
            const m = parseInt(e.target.dataset.month);
            const y = parseInt(e.target.dataset.year);
            onSelectDate(new Date(y, m, d));
            e.preventDefault();
        }
        
        if (e.target.classList.contains('date-picker-prev')) {
            const newMonth = month === 0 ? 11 : month - 1;
            const newYear = month === 0 ? year - 1 : year;
            renderCalendar(container, new Date(newYear, newMonth, 1), locale, onSelectDate);
            e.preventDefault();
        }
        
        if (e.target.classList.contains('date-picker-next')) {
            const newMonth = month === 11 ? 0 : month + 1;
            const newYear = month === 11 ? year + 1 : year;
            renderCalendar(container, new Date(newYear, newMonth, 1), locale, onSelectDate);
            e.preventDefault();
        }
    });
    
    container.addEventListener('change', (e) => {
        if (e.target.classList.contains('date-picker-month-select')) {
            const newMonth = parseInt(e.target.value);
            renderCalendar(container, new Date(year, newMonth, 1), locale, onSelectDate);
        }
        
        if (e.target.classList.contains('date-picker-year-select')) {
            const newYear = parseInt(e.target.value);
            renderCalendar(container, new Date(newYear, month, 1), locale, onSelectDate);
        }
    });
}

export function initializeDatePicker(pickerElement) {
    const locale = detectLocale();
    const displayInput = pickerElement.querySelector('[data-date-input]');
    const hiddenInput = pickerElement.querySelector('[data-hidden-value]');
    const calendarContainer = pickerElement.querySelector('[data-calendar]');
    const toggleButton = pickerElement.querySelector('.date-picker-toggle');
    const includeTime = pickerElement.dataset.includeTime === 'true';
    const timeOnly = pickerElement.dataset.timeOnly === 'true';
    
    let currentDate = new Date();
    
    if (hiddenInput.value) {
        const parsed = new Date(hiddenInput.value);
        // Ignore obviously invalid or placeholder dates (guard against bad server formatting)
        if (!isNaN(parsed) && parsed.getFullYear() >= 1900) {
            currentDate = parsed;
        }
    }
    
    if (displayInput.value) {
        const parsed = parseDate(displayInput.value, locale);
        if (parsed && parsed.getFullYear() >= 1900) {
            currentDate = parsed;
            hiddenInput.value = toISOString(currentDate);
        }
    }
    
    displayInput.value = formatDate(currentDate, locale, includeTime);
    
    function updateDisplay(date) {
        currentDate = date;
        displayInput.value = formatDate(date, locale, includeTime);
        hiddenInput.value = toISOString(date);
        
        hiddenInput.dispatchEvent(new Event('change', { bubbles: true }));
        displayInput.classList.remove('is-invalid');
        
        calendarContainer.setAttribute('hidden', '');
    }
    
    function showCalendar() {
        calendarContainer.removeAttribute('hidden');
        renderCalendar(calendarContainer, currentDate, locale, updateDisplay);
    }
    
    function hideCalendar() {
        calendarContainer.setAttribute('hidden', '');
    }
    
    toggleButton.addEventListener('click', (e) => {
        e.preventDefault();
        if (calendarContainer.hasAttribute('hidden')) {
            showCalendar();
        } else {
            hideCalendar();
        }
    });
    
    displayInput.addEventListener('focus', showCalendar);
    
    displayInput.addEventListener('blur', (e) => {
        setTimeout(() => {
            if (!pickerElement.contains(document.activeElement)) {
                hideCalendar();
                
                if (displayInput.value.trim() !== '') {
                    const parsed = parseDate(displayInput.value, locale);
                    if (parsed) {
                        updateDisplay(parsed);
                    } else {
                        displayInput.classList.add('is-invalid');
                    }
                }
            }
        }, 100);
    });
    
    displayInput.addEventListener('input', () => {
        const parsed = parseDate(displayInput.value, locale);
        if (parsed) {
            currentDate = parsed;
            hiddenInput.value = toISOString(parsed);
            displayInput.classList.remove('is-invalid');
        } else if (displayInput.value.trim() !== '') {
            displayInput.classList.add('is-invalid');
        } else {
            displayInput.classList.remove('is-invalid');
        }

        hiddenInput.dispatchEvent(new Event('change', { bubbles: true }));
    });
    
    document.addEventListener('click', (e) => {
        if (!pickerElement.contains(e.target)) {
            hideCalendar();
        }
    });
}
