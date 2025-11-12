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

Each component should:
- Inherit from appropriate base classes (e.g., `GH_OasysComponent`)
- Implement proper input/output parameter registration
- Include appropriate icons from `Properties/Icons/`
- Handle units consistently using OasysUnits

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
