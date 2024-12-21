// 1. Extend built-in Date object
//    d.addDays(days) --> return new Date object after days added
Date.prototype.addDays = function (days) {
    const d = new Date(this);
    d.setDate(d.getDate() + days);
    return d;
};

// 2. Extend built-in Date object
//    d.format() --> return value in YYYY-MM-DD format
Date.prototype.format = function () {
    return this.toISOString().substring(0, 10);
};

// 3. #CheckIn input event
//    --> Update #CheckOut min = #CheckIn +  1 day
//    --> Update #CheckOut max = #CheckIn + 10 days
//    --> Adjust #CheckOut value (within range)
$('#CheckInDate').on('input', e => {
    const ci = $('#CheckInDate')[0];
    const co = $('#CheckOutDate')[0];

    co.min = ci.valueAsDate.addDays(1).format();
    co.max = ci.valueAsDate.addDays(10).format();

    if (co.value < co.min) co.value = co.min;
    if (co.value > co.max) co.value = co.max;
});

// 4. #CheckIn, #CheckOut input event
//    --> Calculate days difference
//    --> 1 day = 24 * 60 * 60 * 1000 ms
$('#CheckInDate,#CheckOutDate').on('input', e => {
    const ci = $('#CheckInDate')[0];
    const co = $('#CheckOutDate')[0];
    const ms = co.valueAsDate - ci.valueAsDate;
    const days = parseInt(ms / (24 * 60 * 60 * 1000));
    $('#days').text(days);
});

// 5. Run #CheckIn input event for once
$('#CheckInDate').trigger('input');