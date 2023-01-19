# User Features

## Addresses

During checkout you can save the shipping and billing addresses for later use. This can be accessed in the `/user/addresses` page. You can also fill these before checkout, perhaps direct the user to this page after registration.

## Permissions

By default all permissions are admin-only. This is fine for the management permissions (Manage Currency Settings, Manage Stripe API Settings, Manage Region Settings).

There is also the _Ability to checkout_ permission. This is required to complete a checkout. It's also admin-only to avoid being too permissive. You should add it to the role you want to be able to purchase (typically this is the _Authenticated_ role).
