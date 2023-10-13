describe('Register', () => {

    // Test successful registration from the main page
    it('Should Successfully Register new User', function() {
        cy.visit('http://localhost:3500/');
        cy.url().should('include', '/');
    cy.get('button[aria-label="Menu"]').click();
    cy.get('button[aria-label="Menu"]').click();
    cy.get('button').contains('Register').click();
        cy.get('input[id="email"]').type('testRegister@email.com');
        cy.get('input[id="username"]').type('TestUsername');
        cy.get('input[id="password"]').type('password123');
        cy.get('input[id="confirmPassword"]').type('password123');
        cy.get('input[id="date"]').click().type('2000-01-01');
        cy.get('button[type="submit"]').click();
        cy.url().should('include', '/MyProfile');
    });

    // Test unsuccessful registration from the main page
    it('Should not Register new User with existing email', function() {
        cy.visit('http://localhost:3500/');
        cy.url().should('include', '/');
    cy.get('button[aria-label="Menu"]').click();
    cy.get('button[aria-label="Menu"]').click();
    cy.get('button').contains('Register').click();
        cy.get('input[id="email"]').type('testRegister@email.com');
        cy.get('input[id="username"]').type('TestUsername');
        cy.get('input[id="password"]').type('password123');
        cy.get('input[id="confirmPassword"]').type('password123');
        cy.get('input[id="date"]').click().type('2000-01-01');
        cy.get('button[type="submit"]').click();
        cy.url().should('include', '/auth/Signup');
        cy.contains('Registration failed.').should('be.visible');
    });

    // Test unsuccessful registration from the main page. Passwords do not match.
    it('Should not Register new User Passwords that do not match', function() {
        cy.visit('http://localhost:3500/');
        cy.url().should('include', '/');
    cy.get('button[aria-label="Menu"]').click();
    cy.get('button[aria-label="Menu"]').click();
    cy.get('button').contains('Register').click();
        cy.get('input[id="email"]').type('testNewRegister@email.com');
        cy.get('input[id="username"]').type('TestUsername');
        cy.get('input[id="password"]').type('password123');
        cy.get('input[id="confirmPassword"]').type('password1234');
        cy.get('input[id="date"]').click().type('2000-01-01');
        cy.get('button[type="submit"]').click();
        cy.url().should('include', '/auth/Signup');
        cy.contains('Passwords do not match.').should('be.visible');
    });

});