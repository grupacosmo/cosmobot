# Cosmobot

## Requirements

- [Git LFS](https://git-lfs.com/)
- Unity 6000.0.50f1 LTS

# Project File structure

- Animations
- Audio
- Scenes
- Materials
- Models
- Scripts
  - _Main systems per dir_ 
- Shaders
- Prefabs
- Textures
  - _dir per prefab "group"_

# Contributing

## Branching

- The `main` branch is the primary development branch.
- All development work should be done in feature branches.
- Branch names should be descriptive and follow the pattern: `feat/issue-number/your-feature` or 
  `fix/issue-number/your-bug-fix`, e.g. `feat/1/player-movement`.
- Branches should be merged into `main` using a pull request (PR).
- Branches should be deleted after merging.

## Pull Requests

- Before creating a pull request (PR), ensure that your branch is up to date with the latest changes
  from the `main` branch.
- Each PR should address a single issue.
- PR titles and commit messages should follow the 
  [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) include issue number and be
  concise and descriptive, summarizing the changes made, e.g. `feat(#1): add player movement`.
- Reference the related issue in your PR description using the syntax `Fixes #issueNumber` or 
  `Closes #issueNumber` to automatically close the linked issue when the PR is merged.
- The PR should be reviewed by at least one other team member before merging.
- The PR should be merged by the person who created it.
- Add appropriate "_type:_" labels to the PR to indicate the type of change made, e.g. `type: bug`, 
  `type: feature`, etc.