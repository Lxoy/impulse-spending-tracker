// Custom unobtrusive validator: daterange
(function ($) {
    if (!$ || !$.validator || !$.validator.unobtrusive) return;

    function revalidateDateRangeTargets(changedInput) {
        var form = $(changedInput).closest('form');
        if (!form.length) {
            return;
        }

        var changedName = changedInput.name;
        var relatedTargets = form.find('input[data-val-daterange-from="' + changedName + '"], input[data-val-daterange-to="' + changedName + '"]');
        if (!relatedTargets.length) {
            return;
        }

        var validator = form.data('validator') || form.validate();
        validator.element(changedInput);
        relatedTargets.each(function () {
            validator.element(this);
        });
    }

    $.validator.addMethod('daterange', function (value, element, params) {
        var form = $(element).closest('form');
        var fromName = params.from;
        var toName = params.to;
        var fromVal = form.find("input[name='" + fromName + "']").val();
        var toVal = form.find("input[name='" + toName + "']").val();
        if (!fromVal || !toVal) return true; // let required handle empties
        var fromDate = Date.parse(fromVal);
        var toDate = Date.parse(toVal);
        if (isNaN(fromDate) || isNaN(toDate)) return true;
        return fromDate <= toDate;
    });

    $.validator.unobtrusive.adapters.add('daterange', ['from', 'to'], function (options) {
        options.rules['daterange'] = { from: options.params.from, to: options.params.to };
        options.messages['daterange'] = options.message;
    });

    $(document).on('change', 'input.date-picker-value', function () {
        revalidateDateRangeTargets(this);
    });
})(window.jQuery);
