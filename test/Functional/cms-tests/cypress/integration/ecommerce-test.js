/// <reference types="Cypress" />
import { generateTenantInfo } from 'cypress-orchardcore/dist/utils';

describe('Ecommerce Recipe test', function () {
    let tenant;

    before(() => {
        tenant = generateTenantInfo("Ecommerce")
        cy.newTenant(tenant);
    })

    it('Displays the home page of the Ecommerce theme', function(){
        cy.visit(`${tenant.prefix}`);
        cy.get('h4').should('contain.text', 'Welcome to the Orchard Framework, your site has been successfully set up.');
    })

    it('SaaS admin login should work', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin`);
        cy.get('.ta-content').should('contain.text', 'Welcome to Orchard')
    })
});
