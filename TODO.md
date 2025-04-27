# The large todo list

## ✅ Add a way to add changes to the previous commit.

- I should work the same as the commit endpoint
- The docs will be add `sl help amend`

## ✅ Add a way to get the status of the current changes

- The json output from `sl status -Tjson`
- To find out what all the statuses mean you can run `sl help status`
- The status should NOT be a char but a name so the llm can infer from it

## Add a way to list pull requests from github

I think this will need to be a separate tool file.

- Get the open pull requests on the current repo.
- Get all the comments and reviews on a pr
- Get the CI status of the current pr.
