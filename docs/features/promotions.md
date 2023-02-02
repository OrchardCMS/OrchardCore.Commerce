# Promotions

Currently you can apply a percentage or flat amount promotion for each product individually, if their content type has the _Discount_ content part.

1. Enable the _Orchard Core Commerce - Promotion_ feature.
2. Edit the product in the content item editor.
3. Set either the _Discount Percentage_ or _Discount Amount_ fields. You can't set both.
4. You may set the _Beginning_ and _Expiration_ fields for a timed discount period.
5. Set the _Maximum Products_ field if you want to limit how many products can be bought discounted in one order.
6. Set the _Minimum Products_ field if you only want to discount bulk orders.

## Global Discounts

You can define discounts that apply to every product.

1. Create a new content type.
2. Set the _Stereotype_ to `GlobalDiscount`. This is how the promotion provider identifies the content type as a global discount source.
3. Add a _Discount_ content part.
4. Create content items using this type and define the discount parameters described above.

You can limit the discount to a certain group of users by role:
1. Edit the content type and make it _Securable_.
2. Go to the _Roles_ menu and select the role you want to permit.
3. Find the content type permissions and allow the "List content item(s) owned by all users" option. We use this content type permission, because the view permission is effective granted for everyone (even Anonymous) by default so that's not useful. Also because it doesn't imply edit privileges so it's harmless for this purpose.

You can create multiple global discount content types if you want to grant discounts to several roles.
