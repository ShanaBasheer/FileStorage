/// <reference types="cypress" />

describe('File Storage E2E Suite', () => {

  it('Full Flow: Login -> File List -> Download', () => {

    // LOGIN
    cy.visit('/login');
    cy.get('input[name="username"]').type('admin');
    cy.get('input[name="password"]').type('admin123');
    cy.get('button[type="submit"]').click();
    cy.url().should('include', '/home');

    // GO TO FILE LIST
    cy.contains('View Files').click();
    cy.url().should('include', '/file-list');

    // CHECK FILE ROWS
    cy.get('table.file-table tbody tr', { timeout: 10000 })
      .should('exist');

    // DOWNLOAD
    cy.intercept('GET', '**/api/files/*/download**').as('download');

    cy.get('table.file-table tbody tr')
      .first()
      .find('button.download')
      .click();

    cy.wait('@download')
      .its('response.statusCode')
      .should('eq', 200);

  });

});