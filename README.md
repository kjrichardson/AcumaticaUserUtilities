# AcumaticaUserUtilities
Acumatica User Utilities is a collection of utilities that I use to manage user accounts in Acumatica.

## Usage

### Convert AD Roles to Local Roles
This action is available in the user management screen. It converts roles from Active Directory to local roles for the selected user. This is helpful when you wish to override user roles, but want to copy the existing roles.

### Convert AD User to Local User
This action migrates an AD user to a local user. Ensure the user has a valid email address before performing this action. 

### Copy User Favorites
Use the "Copy User Favorites" action to copy favorite screens, tiles, and pinned screens from another user.


### Add roles to Login Types
For LoginTypes, use either Add All Roles, Add Employee Roles, or Add Guest Roles to bring the existing roles into the allowed roles. This helps speeding up adding multiple login types while having multiple roles.

## Dependencies

- **Acumatica ERP Framework**: This project is built as an extension for Acumatica ERP.
- **.NET Framework 4.8**: Ensure your environment supports this version.

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository.
2. Create a feature branch.
3. Commit your changes.
4. Submit a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
