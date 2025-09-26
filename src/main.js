import Game from './Game.js';

/**
 * Main entry point for the game
 * Initializes the game when the page loads
 */
window.addEventListener('load', () => {
    try {
        console.log('Starting Angry Dogs game...');
        const game = new Game();
        console.log('Game initialized successfully');
        
        // Make game available globally for debugging
        window.game = game;
        
    } catch (error) {
        console.error('Failed to initialize game:', error);
        
        // Show error message to user
        const canvas = document.getElementById('gameCanvas');
        const ctx = canvas.getContext('2d');
        
        ctx.fillStyle = '#ff0000';
        ctx.font = '24px Arial';
        ctx.textAlign = 'center';
        ctx.fillText('Game failed to load', canvas.width / 2, canvas.height / 2);
        ctx.fillText('Check console for details', canvas.width / 2, canvas.height / 2 + 30);
    }
});