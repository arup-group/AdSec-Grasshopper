# Copilot Instructions for AdSec-Grasshopper

## Project Overview

AdSec-Grasshopper is a Grasshopper plugin that wraps Oasys AdSec's .NET API. The plugin enables Grasshopper users to create, edit, and analyze AdSec concrete section models seamlessly within the Grasshopper environment.

## Repository Structure

- `AdSecGH/` - Main Grasshopper plugin project containing components, parameters, and UI elements
- `AdSecCore/` - Core functionality shared between projects (builders, parameters, helpers)
- `AdSecGHTests/` - Unit tests for the Grasshopper plugin
- `AdSecCoreTests/` - Unit tests for core functionality
- `IntegrationTests/` - Integration tests
- `ExampleFiles/` - Grasshopper example files used for testing and documentation

## Technology Stack

- **Language**: C# (.NET Framework 4.8)
- **Target Platform**: x64 only
- **Key Dependencies**:
  - AdSec API (v2.0.4.234) - Requires licensed version to load
  - Grasshopper SDK (v6.27)
  - RhinoCommon (v6.27)
  - OasysGH (v1.2.4) - Oasys Grasshopper utilities
  - OasysUnits (v1.2.1) - Unit handling
  - Newtonsoft.Json (v13.0.3)

## Code Style and Conventions

- Follow the `.editorconfig` settings:
  - 2 spaces for indentation
  - LF line endings
  - System directives sorted first with import groups separated
  - Avoid `this.` qualification
  - Use language keywords over BCL types
  - Always add parentheses for clarity in binary operations
  - Require accessibility modifiers for non-interface members

## Building the Project

### Prerequisites
- Visual Studio 2019 or later with .NET Framework 4.8 SDK
- Rhino 7 installed (for debugging)
- MSBuild for command-line builds

### Build Commands
```bash
# Build the solution
msbuild /p:AppxBundlePlatforms="x64" /p:AppxBundle=Always /m AdSecGH.sln

# Format code
dotnet format --exclude-diagnostics --verify-no-changes --verbosity diagnostic AdSecGH.sln
```

### Build Output
The build process:
1. Compiles AdSecGH.dll
2. Copies it to AdSec.gha (Grasshopper assembly format)
3. Copies OasysUnits.dll to output directory
4. Copies AdSecIcon.png for plugin branding

## Testing

### Running Tests
```powershell
# Run tests with coverage
.\coverage.ps1
```

### Test Projects
- `AdSecGHTests` - Unit tests for Grasshopper components
- `AdSecCoreTests` - Unit tests for core functionality
- `IntegrationTests` - End-to-end integration tests

## Development Workflow

### Pre-commit Hooks
This project uses pre-commit hooks for quality control:
- **pre-commit**: Code formatting with `dotnet format`
- **pre-push**: Build verification and test coverage
- **commit-msg**: Commitizen for conventional commits

### Setup Development Environment
```bash
# Setup virtual environment
python -m venv .venv
.\.venv\Scripts\activate

# Install pre-commit
pip install pre-commit

# Install commitizen
pip install Commitizen

# Install hooks
pre-commit install --hook-type commit-msg --hook-type pre-push
pre-commit install
```

## Component Structure

Grasshopper components are organized by category in `AdSecGH/Components/`:
- Category 1-2: Input components
- Category 3: Section creation and manipulation
- Category 4-5: Analysis and properties
- Category 6: Results and visualization
- Lab: Experimental features

### Component Architecture Pattern

Components follow a specific inheritance hierarchy with business logic in `AdSecCore` and Grasshopper adapters in `AdSecGH`:

1. **Core Function Layer** (`AdSecCore/Functions/`):
   - Base class: `Function` (abstract class in `IFunction.cs`)
   - Example: `CreateRebarFunction : Function`
   - Contains business logic, parameters, and computation
   - Defines `Metadata.Name` property for component identification

2. **Grasshopper Adapter Layer** (`AdSecGH/Components/`):
   - **Direct inheritance** (preferred): `CreateRebar : DropdownAdapter<CreateRebarFunction>`
   - **With shim** (when parameter translation needed): `CreateRebarGh : CreateRebarFunction` then `CreateRebar : DropdownAdapter<CreateRebarGh>`
   - Handles Grasshopper-specific UI, dropdowns, and unit conversions
   - Translates between Grasshopper parameters and AdSecCore simple parameters

3. **Inheritance Priority**:
   - **DO**: Inherit from `Function` base classes (e.g., `CreateRebarFunction`)
   - **DON'T**: Inherit directly from `GH_Component`
   - Use shim classes only when Grasshopper parameter translation is required

### Icon Naming Convention

Icons must be named based on the Function's `Metadata.Name` property using PascalCase:
- `Metadata.Name = "Create Rebar"` → Icon: `Rebar.png` or `CreateRebar.png`
- `Metadata.Name = "Create Section"` → Icon: `CreateSection.png`
- `Metadata.Name = "Create Deformation Load"` → Icon: `CreateDeformationLoad.png`

Each component should:
- Follow the Function inheritance pattern described above
- Include appropriate icons from `Properties/Icons/` with correct PascalCase naming
- Handle units consistently using OasysUnits
- Implement proper input/output parameter registration

## Important Notes

### Licensing
- The plugin requires a licensed version of AdSec API to load
- Contact oasys@arup.com for trial versions
- MIT License for the plugin code itself
- Third-party licenses in `AdSecGH/licenses/` folder

### Debugging
- Debug configuration launches Rhino 7 automatically
- Set breakpoints in component code
- Test with example files in `ExampleFiles/`

### Dependencies
- AdSec API is a commercial dependency
- Always check for compatibility with AdSec API versions
- OasysGH and OasysUnits are maintained by Oasys

## Pull Request Guidelines

1. Update tests as appropriate
2. Ensure code formatting passes (`dotnet format`)
3. Build succeeds on x64 platform
4. All tests pass with adequate coverage
5. Follow conventional commits format
6. Update example files if adding new components

## Code Owners

Default reviewers: @kpne @tlmnrnhrdt

## Additional Resources

- [AdSec API Documentation](https://arup-group.github.io/oasys-combined/adsec-api/)
- [Grasshopper SDK Documentation](https://developer.rhino3d.com/api/grasshopper/)
- Example files: `/ExampleFiles` directory
