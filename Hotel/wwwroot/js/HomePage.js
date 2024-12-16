const roomGrid = document.querySelector('.room-grid');
const scrollSpeed = 5;

roomGrid.addEventListener('wheel', (event) => {
    event.preventDefault();
    roomGrid.scrollLeft += event.deltaY * scrollSpeed;
});