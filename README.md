# Memcached.Namespacer
Namespacing of cache keys allows you to invalidate part of your cache, ie. all key for a specific user ID.

## Why namespacing

Memcached is awesome, because it is lightweight, fast, distributed and thus super scalable. However, this design come with some 
drawbacks. For example, it is not possible to search or even enumerate all keys in the store. These limitations are on purpose
and we need to live with that. One of the things that you often want to be able to do is clearing all cache entries for a specific
user (user ID). A user may create many stored entries while using the site. For example think of an online shop and a user known 
as 12543. At some point in time, the cache could contain:

 - shoppingbasket:12543
 - prodLastWatched:12543:54929873
 - prodLastWatched:12543:92298748
 - prodLastWatched:12543:87391001
 - prodLastWatched:12543:7894234
 - interests:12543
 
 Now if for some reason you want to reset the cache for this specific user 12543, there is really no other way than performing a 
 FLUSH_ALL on memcached, because you have no way to know which keys are in use. You could clear the shopping basket and the 
 interests, but you cannot clear the combination of all product ID's with the user. If you would like to reset all cached data 
 for a specific product, you run into the same problem: you do not know which users may have this stored in cache and you cannot 
 feasably clear this key for all users.
 With namespacing of your keys, you effectively change the ID of a user when used in a cache key. The procedure is simple: for each 
 ID you want to use in your keys, you store a counter in cache. Initially this could be a random number. Then when you want to 
 construct a cache key containing a user ID, you use a combination of the user ID and the counter, which is specific fot 
 this user ID. So the keys in the example above would become something like this:
 
 - shoppingbasket:12543|78567
 - prodLastWatched:12543|78567:54929873|538365
 - prodLastWatched:12543|78567:92298748|355453
 - prodLastWatched:12543|78567:87391001|535223
 - prodLastWatched:12543|78567:7894234|985689
 - interests:12543|78567

Besides these keys, we now also store:

 - counter:userID:12543
 - counter:prodID:54929873
 - counter:prodID:92298748
 - counter:prodID:87391001
 - counter:prodID:7894234

When you want to get something from cache, there is some overhead now: you first have to read from cache the current counter for
this ID, then you use the ID + counter to construct the key. But we have gained an interesting feature:

### Clearing a specific user from cache
If we want to clear cache for a specific user, the only thing we have to do is incrementing the counter in counter:userID:12543. 
This will change all keys that use this user ID. The old cached values will not be used anymore and eventually cleaned up.

## Decreasing overhead

It is not very efficient to have to fetch multiple keys from memcached to read only one entry. There is a way to make this more efficient, especially if clearing is infrequent. What we do is keeping one central master counter that we can use for all namespaces, unless they have changed. This of cource transfers the problem to keeping track of touched namespaces. If namespace changes are uncommon, we can do this in an efficient way. We store some data with the default counter called Evidence. The amount of data used for evidence can change, but for the example, we'll use 8 bytes (64 bits). At start, the bytes are all blank: 0000000000000000. Now if we want to increase the namespace for user ID 12543, we will flip a bit that corresponds to this number. The easiest way it taking the modulo: 12543 % 64 = 63. So we set bit 63 to 1: 0000000000000001.
