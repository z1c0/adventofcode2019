# Advent of Code 2019

https://adventofcode.com/2019

## Day 18

I almost gave up on this one but finally succeeded as my last-but-one (see Day22)
puzzle. For some reason, I really had a hard time wrapping my head around this puzzle.
The trick was using a `PriorityQueue` and clever caching (dynamic programming) as
so often with this kind of way-finding-challenges.

## Day 22

The second part took me way longer than it should have. I had figured out the solution
of how to handle the **huge stack of cards** rather quickly. Then instead of running that
technique for `101741582076661` (in my case) I kept looking for another "clever" solution.
As it turned out, I didn't find one and the runtime of the iterations was not too
bad after all (with printing progress, meh). So instead of trying to be clever,
I should have just run with my first solution.
