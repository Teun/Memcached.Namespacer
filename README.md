# Memcached.Namespacer
Namespacing of cache keys allows you to invalidate part of your cache, ie. all key for a specific user ID.

## Why namespacing

Memcached is awesome, because it is lightweight, fast, distributed and thus super scalable. However, this design come with some 
drawbacks. For example, it is not possible to search or even enumerate all keys in the store. These limitations are on purpose
and we need to live with that. One of the things that you often want to be able to do is clearing all cache entries for a specific
user (user ID) or product or whatever your application is about. A user may create many stored entries while using the site. For 
example think of an online shop and a user known as 12543. At some point in time, the cache could contain:

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

### Doing it yourself

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

It is not very efficient to have to fetch multiple keys from memcached to read only one entry. There is a way to make this more efficient, especially if clearing is infrequent. What we do is keeping one central master counter that we can use for all namespaces, unless they have changed. This of cource transfers the problem to keeping track of touched namespaces. If namespace changes are uncommon, we can do this in an efficient way. We store some data with the default counter called Evidence. The amount of data used for evidence can change, but for the example, we'll use 8 bytes (64 bits). At start, the bytes are all blank: 0000000000000000. Now if we want to increase the namespace for user ID 12543, we will flip a bit that corresponds to this number. The easiest way it taking the modulo: 12543 % 64 = 63. So we set bit 63 to 1: 0000000000000001. From now on, we can still use the master counter for 63 out of 64 keys, but for the rest, we'll have to look up their specific counter from the cache. Actually, we can step up this game once more: we can set multiple bits for each namespace we touch. The first time this will flip 2 (or more) bits, so we use up our evidence more quickly, but the number of keys that touch those exact same two bits is much smaller. Memcached.Namespacer uses 2 bits per key. 

Interestingly, the use of a master counter allows us another interesting feature: rolling flushes. If most keys are determined by the master counter, we can use this mechanism to flush the whole cache (well, the namespace part of the cache) in a more gentle way than calling a FlushAll on your whole cache. 

### FlushAll Rolling
Normally, FlushAll clears your whole cache at once. This is the only way memcached provides to make sure all stale data is removed. If, however, most of our cache keys are namespaced, we have a more gentle option. If we just update the master counter using the current time, but keep the old master counter around for some time, we may use the new master counter (and evidence) for part of the keys, but still refer to the old master counter and evidence for the rest of the namespaced keys. If we do this carefully, we can flush the whole namespace cache over a configurable time span. 

This is actually something you may want to do when the evidence gets polluted by a large number of namespace changes. 

## Usage

Install Memcached.Namespacer using:

    Install-Package Memcached.Namespacer
	

